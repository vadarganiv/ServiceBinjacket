'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';

const CONDITIONS = ['New', 'Used', 'Refurbished', 'Unknown'];

interface Category {
  id: number;
  nameSq: string;
}

interface ProductFormData {
  nameSq: string;
  nameEn: string;
  slugSq: string;
  slugEn: string;
  shortDescriptionSq: string;
  shortDescriptionEn: string;
  descriptionSq: string;
  descriptionEn: string;
  price: string;
  currency: string;
  condition: string;
  stockQty: string;
  categoryId: string;
  warrantyMonths: string;
  isPublished: boolean;
}

interface Props {
  categories: Category[];
  initialData?: Partial<ProductFormData> & { id?: number };
  mode: 'create' | 'edit';
}

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

export default function ProductForm({ categories, initialData, mode }: Props) {
  const router = useRouter();

  const [form, setForm] = useState<ProductFormData>({
    nameSq: initialData?.nameSq ?? '',
    nameEn: initialData?.nameEn ?? '',
    slugSq: initialData?.slugSq ?? '',
    slugEn: initialData?.slugEn ?? '',
    shortDescriptionSq: initialData?.shortDescriptionSq ?? '',
    shortDescriptionEn: initialData?.shortDescriptionEn ?? '',
    descriptionSq: initialData?.descriptionSq ?? '',
    descriptionEn: initialData?.descriptionEn ?? '',
    price: String(initialData?.price ?? ''),
    currency: initialData?.currency ?? 'ALL',
    condition: initialData?.condition ?? 'New',
    stockQty: String(initialData?.stockQty ?? ''),
    categoryId: String(initialData?.categoryId ?? ''),
    warrantyMonths: String(initialData?.warrantyMonths ?? ''),
    isPublished: initialData?.isPublished ?? false,
  });

  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  function set(field: keyof ProductFormData, value: string | boolean) {
    setForm(prev => ({ ...prev, [field]: value }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSaving(true);
    setError('');

    const payload = {
      nameSq: form.nameSq,
      nameEn: form.nameEn || null,
      slugSq: form.slugSq || null,
      slugEn: form.slugEn || null,
      shortDescriptionSq: form.shortDescriptionSq,
      shortDescriptionEn: form.shortDescriptionEn || null,
      descriptionSq: form.descriptionSq,
      descriptionEn: form.descriptionEn || null,
      price: parseFloat(form.price) || 0,
      currency: form.currency,
      condition: form.condition,
      stockQty: form.stockQty !== '' ? parseInt(form.stockQty) : null,
      categoryId: parseInt(form.categoryId),
      warrantyMonths: form.warrantyMonths !== '' ? parseInt(form.warrantyMonths) : null,
      isPublished: form.isPublished,
    };

    const url = mode === 'create'
      ? `${API_URL}/api/v1/admin/products`
      : `${API_URL}/api/v1/admin/products/${initialData?.id}`;

    const res = await fetch(url, {
      method: mode === 'create' ? 'POST' : 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(payload),
    });

    setSaving(false);

    if (res.ok) {
      const data = await res.json();
      router.push(`/admin/products/${data.id}`);
      router.refresh();
    } else {
      const data = await res.json().catch(() => ({}));
      setError(data?.error?.message ?? 'An error occurred');
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Albanian / English side-by-side */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Albanian column */}
        <div className="bg-white rounded-lg border p-4 space-y-4">
          <h3 className="font-medium text-gray-900 flex items-center gap-2">
            <span className="text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded font-mono">sq</span>
            Albanian (primary)
          </h3>
          <Field label="Name (SQ) *">
            <input value={form.nameSq} onChange={e => set('nameSq', e.target.value)}
              className={input} required placeholder="Emri i produktit" />
          </Field>
          <Field label="Slug SQ (auto if empty)">
            <input value={form.slugSq} onChange={e => set('slugSq', e.target.value)}
              className={input} placeholder="auto-generated" />
          </Field>
          <Field label="Short description (SQ) *">
            <textarea value={form.shortDescriptionSq} onChange={e => set('shortDescriptionSq', e.target.value)}
              className={`${input} resize-none`} rows={2} required />
          </Field>
          <Field label="Description (SQ)">
            <textarea value={form.descriptionSq} onChange={e => set('descriptionSq', e.target.value)}
              className={`${input} resize-none`} rows={5} />
          </Field>
        </div>

        {/* English column */}
        <div className="bg-white rounded-lg border p-4 space-y-4">
          <h3 className="font-medium text-gray-900 flex items-center gap-2">
            <span className="text-xs bg-gray-100 text-gray-700 px-2 py-0.5 rounded font-mono">en</span>
            English (optional)
          </h3>
          <Field label="Name (EN)">
            <input value={form.nameEn} onChange={e => set('nameEn', e.target.value)}
              className={input} placeholder="Product name" />
          </Field>
          <Field label="Slug EN (auto if empty)">
            <input value={form.slugEn} onChange={e => set('slugEn', e.target.value)}
              className={input} placeholder="auto-generated" />
          </Field>
          <Field label="Short description (EN)">
            <textarea value={form.shortDescriptionEn} onChange={e => set('shortDescriptionEn', e.target.value)}
              className={`${input} resize-none`} rows={2} />
          </Field>
          <Field label="Description (EN)">
            <textarea value={form.descriptionEn} onChange={e => set('descriptionEn', e.target.value)}
              className={`${input} resize-none`} rows={5} />
          </Field>
        </div>
      </div>

      {/* Pricing & details */}
      <div className="bg-white rounded-lg border p-4">
        <h3 className="font-medium text-gray-900 mb-4">Pricing & Details</h3>
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
          <Field label="Price *">
            <input type="number" min="0" step="0.01" value={form.price}
              onChange={e => set('price', e.target.value)}
              className={input} required placeholder="0" />
          </Field>
          <Field label="Currency">
            <input value={form.currency} onChange={e => set('currency', e.target.value)}
              className={input} maxLength={10} />
          </Field>
          <Field label="Condition">
            <select value={form.condition} onChange={e => set('condition', e.target.value)} className={input}>
              {CONDITIONS.map(c => <option key={c} value={c}>{c}</option>)}
            </select>
          </Field>
          <Field label="Category *">
            <select value={form.categoryId} onChange={e => set('categoryId', e.target.value)}
              className={input} required>
              <option value="">-- select --</option>
              {categories.map(c => (
                <option key={c.id} value={c.id}>{c.nameSq}</option>
              ))}
            </select>
          </Field>
          <Field label="Stock qty (null = unlimited)">
            <input type="number" min="0" value={form.stockQty}
              onChange={e => set('stockQty', e.target.value)}
              className={input} placeholder="unlimited" />
          </Field>
          <Field label="Warranty (months)">
            <input type="number" min="1" value={form.warrantyMonths}
              onChange={e => set('warrantyMonths', e.target.value)}
              className={input} placeholder="none" />
          </Field>
          <Field label="Published">
            <div className="flex items-center h-9">
              <input type="checkbox" id="isPublished" checked={form.isPublished}
                onChange={e => set('isPublished', e.target.checked)}
                className="w-4 h-4 text-blue-600 rounded" />
              <label htmlFor="isPublished" className="ml-2 text-sm text-gray-700">Visible to public</label>
            </div>
          </Field>
        </div>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg px-4 py-3 text-sm text-red-700">{error}</div>
      )}

      <div className="flex gap-3">
        <button
          type="submit"
          disabled={saving}
          className="px-6 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors"
        >
          {saving ? 'Saving...' : mode === 'create' ? 'Create product' : 'Save changes'}
        </button>
        <button
          type="button"
          onClick={() => router.back()}
          className="px-6 py-2 bg-white text-gray-700 text-sm font-medium rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
        >
          Cancel
        </button>
      </div>
    </form>
  );
}

const input = 'w-full text-sm border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white';

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div>
      <label className="block text-xs font-medium text-gray-600 mb-1">{label}</label>
      {children}
    </div>
  );
}
