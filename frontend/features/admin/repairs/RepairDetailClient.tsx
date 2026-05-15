'use client';

import { useState } from 'react';

const REPAIR_STATUSES = [
  'New', 'Contacted', 'WaitingForDevice', 'Received', 'Diagnostics',
  'PriceOffered', 'PriceAgreed', 'Repairing', 'Ready', 'SentBack',
  'Completed', 'Cancelled',
];

interface RepairFile {
  id: number;
  path: string;
  originalName: string;
  mimeType: string;
  sizeBytes: number;
  uploadedAt: string;
}

interface RepairDetail {
  id: number;
  status: string;
  adminComment: string | null;
  createdAt: string;
  updatedAt: string;
  customer: {
    fullName: string;
    phone: string;
    whatsAppPhone: string | null;
    city: string;
  };
  service: { id: number; name: string } | null;
  deviceType: string;
  brand: string | null;
  model: string | null;
  problemDescription: string;
  preferredDeliveryMethod: string;
  customerComment: string | null;
  files: RepairFile[];
}

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

export default function RepairDetailClient({ repair }: { repair: RepairDetail }) {
  const [status, setStatus] = useState(repair.status);
  const [comment, setComment] = useState(repair.adminComment ?? '');
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState('');

  const waPhone = repair.customer.whatsAppPhone ?? repair.customer.phone;
  const whatsappLink = `https://wa.me/${waPhone.replace(/\D/g, '')}`;

  async function saveStatus() {
    setSaving(true);
    setMsg('');
    try {
      const res = await fetch(`${API_URL}/api/v1/admin/repair-requests/${repair.id}/status`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ status }),
      });
      setMsg(res.ok ? 'Status updated' : 'Error updating status');
    } finally {
      setSaving(false);
    }
  }

  async function saveComment() {
    setSaving(true);
    setMsg('');
    try {
      const res = await fetch(`${API_URL}/api/v1/admin/repair-requests/${repair.id}/comment`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ comment }),
      });
      setMsg(res.ok ? 'Comment saved' : 'Error saving comment');
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Repair Request #{repair.id}</h1>
          <p className="text-sm text-gray-500 mt-1">{new Date(repair.createdAt).toLocaleString()}</p>
        </div>
        <a
          href={whatsappLink}
          target="_blank"
          rel="noopener noreferrer"
          className="inline-flex items-center gap-2 px-4 py-2 bg-green-500 hover:bg-green-600 text-white text-sm font-medium rounded-lg transition-colors"
        >
          <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
            <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347z" />
            <path d="M11.99 0C5.375 0 0 5.373 0 12c0 2.117.553 4.103 1.518 5.829L0 24l6.335-1.652A11.954 11.954 0 0011.99 24C18.614 24 24 18.627 24 12S18.614 0 11.99 0zm.01 21.818a9.817 9.817 0 01-5.006-1.362l-.36-.214-3.732.979 1.001-3.648-.235-.374A9.818 9.818 0 012.182 12c0-5.42 4.41-9.818 9.818-9.818 5.42 0 9.818 4.41 9.818 9.818 0 5.42-4.398 9.818-9.818 9.818z" />
          </svg>
          WhatsApp
        </a>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Customer */}
        <div className="bg-white rounded-lg border p-4">
          <h2 className="font-medium text-gray-900 mb-3">Customer</h2>
          <div className="space-y-1 text-sm">
            <p className="font-medium">{repair.customer.fullName}</p>
            <p className="text-gray-600">{repair.customer.phone}</p>
            {repair.customer.whatsAppPhone && <p className="text-gray-600">WA: {repair.customer.whatsAppPhone}</p>}
            <p className="text-gray-600">{repair.customer.city}</p>
          </div>
        </div>

        {/* Device info */}
        <div className="bg-white rounded-lg border p-4">
          <h2 className="font-medium text-gray-900 mb-3">Device</h2>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between">
              <span className="text-gray-500">Type</span>
              <span>{repair.deviceType}</span>
            </div>
            {repair.brand && <div className="flex justify-between"><span className="text-gray-500">Brand</span><span>{repair.brand}</span></div>}
            {repair.model && <div className="flex justify-between"><span className="text-gray-500">Model</span><span>{repair.model}</span></div>}
            {repair.service && <div className="flex justify-between"><span className="text-gray-500">Service</span><span>{repair.service.name}</span></div>}
            <div className="flex justify-between">
              <span className="text-gray-500">Delivery pref.</span>
              <span>{repair.preferredDeliveryMethod}</span>
            </div>
          </div>
          <div className="mt-3 pt-3 border-t">
            <p className="text-xs text-gray-500 mb-1">Problem description</p>
            <p className="text-sm text-gray-700">{repair.problemDescription}</p>
          </div>
          {repair.customerComment && (
            <div className="mt-2 pt-2 border-t">
              <p className="text-xs text-gray-500 mb-1">Customer note</p>
              <p className="text-sm text-gray-700">{repair.customerComment}</p>
            </div>
          )}
        </div>

        {/* Status + comment */}
        <div className="bg-white rounded-lg border p-4 space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
            <div className="flex gap-2">
              <select
                value={status}
                onChange={e => setStatus(e.target.value)}
                className="flex-1 text-sm border border-gray-300 rounded-md px-2 py-1.5 focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                {REPAIR_STATUSES.map(s => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
              <button
                onClick={saveStatus}
                disabled={saving}
                className="px-3 py-1.5 bg-blue-600 text-white text-sm rounded-md hover:bg-blue-700 disabled:opacity-50"
              >
                Save
              </button>
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Admin comment</label>
            <textarea
              value={comment}
              onChange={e => setComment(e.target.value)}
              rows={3}
              className="w-full text-sm border border-gray-300 rounded-md px-2 py-1.5 focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
            />
            <button
              onClick={saveComment}
              disabled={saving}
              className="mt-2 w-full py-1.5 bg-gray-700 text-white text-sm rounded-md hover:bg-gray-800 disabled:opacity-50"
            >
              Save comment
            </button>
          </div>
          {msg && <p className="text-sm text-green-600">{msg}</p>}
        </div>
      </div>

      {/* Files */}
      {repair.files.length > 0 && (
        <div className="bg-white rounded-lg border overflow-hidden">
          <h2 className="font-medium text-gray-900 px-4 py-3 border-b">Attached Files ({repair.files.length})</h2>
          <div className="divide-y divide-gray-200">
            {repair.files.map(f => {
              const isImage = f.mimeType.startsWith('image/');
              const fileUrl = `${API_URL}${f.path}`;
              return (
                <div key={f.id} className="flex items-center gap-4 px-4 py-3">
                  {isImage && (
                    <a href={fileUrl} target="_blank" rel="noopener noreferrer">
                      {/* eslint-disable-next-line @next/next/no-img-element */}
                      <img
                        src={fileUrl}
                        alt={f.originalName}
                        className="w-14 h-14 object-cover rounded border"
                      />
                    </a>
                  )}
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-gray-900 truncate">{f.originalName}</p>
                    <p className="text-xs text-gray-500">{f.mimeType} · {formatBytes(f.sizeBytes)}</p>
                  </div>
                  <a
                    href={fileUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-sm text-blue-600 hover:text-blue-800 font-medium shrink-0"
                  >
                    Open ↗
                  </a>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
