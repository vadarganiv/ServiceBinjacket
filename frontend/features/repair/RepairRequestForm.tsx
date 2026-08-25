'use client';

import { useState, useRef } from 'react';
import { useTranslations } from 'next-intl';
import Link from 'next/link';
import type { ServiceListItem, RepairRequestResponse } from '@/lib/types';

interface Props {
  locale: string;
  services: ServiceListItem[];
}

interface FormErrors {
  fullName?: string;
  phone?: string;
  city?: string;
  deviceType?: string;
  problemDescription?: string;
  consent?: string;
  files?: string;
  server?: string;
}

const MAX_FILES = 4;
const MAX_FILE_BYTES = 25 * 1024 * 1024;
const MAX_TOTAL_BYTES = 30 * 1024 * 1024;

export default function RepairRequestForm({ locale, services }: Props) {
  const t = useTranslations('repairRequest');
  const tSuccess = useTranslations('success');

  const [submitting, setSubmitting] = useState(false);
  const [errors, setErrors] = useState<FormErrors>({});
  const [successId, setSuccessId] = useState<number | null>(null);
  const [successWarning, setSuccessWarning] = useState('');
  const [selectedFiles, setSelectedFiles] = useState<FileList | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [form, setForm] = useState({
    fullName: '',
    phone: '',
    whatsAppPhone: '',
    city: '',
    serviceId: '',
    deviceType: '',
    brand: '',
    model: '',
    problemDescription: '',
    preferredDeliveryMethod: 'StorePickup',
    customerComment: '',
    consent: false,
  });

  function set(field: string, value: string | boolean) {
    setForm(prev => ({ ...prev, [field]: value }));
    setErrors(prev => ({ ...prev, [field]: undefined, server: undefined }));
  }

  function validate(): FormErrors {
    const errs: FormErrors = {};
    if (!form.fullName.trim()) errs.fullName = t('errorRequired');
    if (!form.phone.trim()) errs.phone = t('errorRequired');
    else if (!/^\+?[0-9\s\-()]{7,20}$/.test(form.phone.trim())) errs.phone = t('errorPhone');
    if (!form.city.trim()) errs.city = t('errorRequired');
    if (!form.deviceType.trim()) errs.deviceType = t('errorRequired');
    if (!form.problemDescription.trim()) errs.problemDescription = t('errorRequired');
    if (!form.consent) errs.consent = t('errorConsent');

    const files = selectedFiles ? Array.from(selectedFiles) : [];
    if (files.length > MAX_FILES) errs.files = t('errorFilesCount');
    else if (files.some(file => file.size > MAX_FILE_BYTES)) errs.files = t('errorFileSize');
    else if (files.reduce((total, file) => total + file.size, 0) > MAX_TOTAL_BYTES) {
      errs.files = t('errorFilesTotalSize');
    }

    return errs;
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const errs = validate();
    if (Object.keys(errs).length > 0) {
      setErrors(errs);
      return;
    }

    setSubmitting(true);
    setErrors({});

    try {
      const body = {
        customer: {
          fullName: form.fullName.trim(),
          phone: form.phone.trim(),
          whatsAppPhone: form.whatsAppPhone.trim() || undefined,
          city: form.city.trim(),
        },
        serviceId: form.serviceId ? Number(form.serviceId) : undefined,
        deviceType: form.deviceType.trim(),
        brand: form.brand.trim() || undefined,
        model: form.model.trim() || undefined,
        problemDescription: form.problemDescription.trim(),
        preferredDeliveryMethod: form.preferredDeliveryMethod,
        customerComment: form.customerComment.trim() || undefined,
        consent: form.consent,
      };

      const res = await fetch('/api/v1/repair-requests', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      });

      if (!res.ok) {
        const data = await res.json().catch(() => null);
        const msg = data?.error?.message ?? t('errorRequired');
        setErrors({ server: msg });
        return;
      }

      const created: RepairRequestResponse = await res.json();

      if (selectedFiles && selectedFiles.length > 0) {
        try {
          const fd = new FormData();
          Array.from(selectedFiles).forEach(f => fd.append('files', f));
          const uploadResponse = await fetch(`/api/v1/repair-requests/${created.id}/files`, {
            method: 'POST',
            body: fd,
          });

          if (!uploadResponse.ok) {
            const uploadError = await uploadResponse.json().catch(() => null);
            setSuccessWarning(uploadError?.error?.message ?? t('errorUploadFailed'));
          }
        } catch {
          setSuccessWarning(t('errorUploadFailed'));
        }
      }

      setSuccessId(created.id);
    } catch {
      setErrors({ server: t('errorNetwork') });
    } finally {
      setSubmitting(false);
    }
  }

  if (successId !== null) {
    return (
      <div className="flex flex-col items-center text-center py-16 px-4 max-w-md mx-auto">
        <div className="w-16 h-16 rounded-full bg-emerald-100 flex items-center justify-center mb-6">
          <svg className="w-8 h-8 text-emerald-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
          </svg>
        </div>
        <h2 className="text-2xl font-bold text-foreground mb-3">{tSuccess('repairRequestTitle')}</h2>
        <p className="text-muted-foreground mb-2">{tSuccess('repairRequestMessage')}</p>
        <p className="text-sm font-medium text-primary mb-8">
          {tSuccess('repairRequestId').replace('{id}', String(successId))}
        </p>
        {successWarning && (
          <p className="text-sm text-amber-700 bg-amber-50 border border-amber-200 rounded-xl px-4 py-3 mb-6">
            {t('requestCreatedUploadWarning')} {successWarning}
          </p>
        )}
        <Link
          href={`/${locale}`}
          className="inline-flex items-center justify-center px-6 py-3 bg-primary text-primary-foreground rounded-xl font-semibold hover:bg-primary/90 transition-colors"
        >
          {tSuccess('backToHome')}
        </Link>
      </div>
    );
  }

  const inputClass = "w-full px-4 py-2.5 text-sm border border-input rounded-xl bg-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring";
  const errorClass = "text-xs text-red-500 mt-1";
  const labelClass = "block text-sm font-medium text-foreground mb-1.5";
  const sectionClass = "space-y-4";

  return (
    <form onSubmit={handleSubmit} className="max-w-2xl mx-auto space-y-8" noValidate>
      {errors.server && (
        <div className="rounded-xl bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">
          {errors.server}
        </div>
      )}

      {/* Customer section */}
      <div className="space-y-4">
        <h2 className="text-lg font-semibold text-foreground border-b border-border pb-2">
          {t('customerSection')}
        </h2>
        <div className={sectionClass}>
          <div>
            <label className={labelClass}>{t('fullName')} *</label>
            <input
              type="text"
              value={form.fullName}
              onChange={e => set('fullName', e.target.value)}
              placeholder={t('fullNamePlaceholder')}
              className={`${inputClass} ${errors.fullName ? 'border-red-400' : ''}`}
            />
            {errors.fullName && <p className={errorClass}>{errors.fullName}</p>}
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className={labelClass}>{t('phone')} *</label>
              <input
                type="tel"
                value={form.phone}
                onChange={e => set('phone', e.target.value)}
                placeholder={t('phonePlaceholder')}
                className={`${inputClass} ${errors.phone ? 'border-red-400' : ''}`}
              />
              {errors.phone && <p className={errorClass}>{errors.phone}</p>}
            </div>
            <div>
              <label className={labelClass}>{t('whatsApp')}</label>
              <input
                type="tel"
                value={form.whatsAppPhone}
                onChange={e => set('whatsAppPhone', e.target.value)}
                placeholder={t('whatsAppPlaceholder')}
                className={inputClass}
              />
            </div>
          </div>

          <div>
            <label className={labelClass}>{t('city')} *</label>
            <input
              type="text"
              value={form.city}
              onChange={e => set('city', e.target.value)}
              placeholder={t('cityPlaceholder')}
              className={`${inputClass} ${errors.city ? 'border-red-400' : ''}`}
            />
            {errors.city && <p className={errorClass}>{errors.city}</p>}
          </div>
        </div>
      </div>

      {/* Device section */}
      <div className="space-y-4">
        <h2 className="text-lg font-semibold text-foreground border-b border-border pb-2">
          {t('deviceSection')}
        </h2>
        <div className={sectionClass}>
          {services.length > 0 && (
            <div>
              <label className={labelClass}>{t('service')}</label>
              <select
                value={form.serviceId}
                onChange={e => set('serviceId', e.target.value)}
                className={inputClass}
              >
                <option value="">{t('serviceSelectDefault')}</option>
                {services.map(s => (
                  <option key={s.id} value={s.id}>{s.name}</option>
                ))}
              </select>
            </div>
          )}

          <div>
            <label className={labelClass}>{t('deviceType')} *</label>
            <input
              type="text"
              value={form.deviceType}
              onChange={e => set('deviceType', e.target.value)}
              placeholder={t('deviceTypePlaceholder')}
              className={`${inputClass} ${errors.deviceType ? 'border-red-400' : ''}`}
            />
            {errors.deviceType && <p className={errorClass}>{errors.deviceType}</p>}
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className={labelClass}>{t('brand')}</label>
              <input
                type="text"
                value={form.brand}
                onChange={e => set('brand', e.target.value)}
                placeholder={t('brandPlaceholder')}
                className={inputClass}
              />
            </div>
            <div>
              <label className={labelClass}>{t('model')}</label>
              <input
                type="text"
                value={form.model}
                onChange={e => set('model', e.target.value)}
                placeholder={t('modelPlaceholder')}
                className={inputClass}
              />
            </div>
          </div>

          <div>
            <label className={labelClass}>{t('problemDescription')} *</label>
            <textarea
              value={form.problemDescription}
              onChange={e => set('problemDescription', e.target.value)}
              placeholder={t('problemDescriptionPlaceholder')}
              rows={4}
              className={`${inputClass} resize-none ${errors.problemDescription ? 'border-red-400' : ''}`}
            />
            {errors.problemDescription && <p className={errorClass}>{errors.problemDescription}</p>}
          </div>

          <div>
            <label className={labelClass}>{t('deliveryMethod')}</label>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
              {[
                { value: 'StorePickup', label: t('deliveryStorePickup') },
                { value: 'LocalCourier', label: t('deliveryLocalCourier') },
                { value: 'PostalShipping', label: t('deliveryPostalShipping') },
                { value: 'ManualAgreement', label: t('deliveryManualAgreement') },
              ].map(opt => (
                <label
                  key={opt.value}
                  className={`flex items-center gap-2.5 px-4 py-3 border rounded-xl cursor-pointer transition-colors ${
                    form.preferredDeliveryMethod === opt.value
                      ? 'border-primary bg-primary/5'
                      : 'border-border hover:border-foreground/30'
                  }`}
                >
                  <input
                    type="radio"
                    name="deliveryMethod"
                    value={opt.value}
                    checked={form.preferredDeliveryMethod === opt.value}
                    onChange={() => set('preferredDeliveryMethod', opt.value)}
                    className="accent-primary"
                  />
                  <span className="text-sm">{opt.label}</span>
                </label>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* Files */}
      <div className="space-y-3">
        <h2 className="text-lg font-semibold text-foreground border-b border-border pb-2">
          {t('filesSection')}
        </h2>
        <p className="text-sm text-muted-foreground">{t('filesHint')}</p>
        <div>
          <input
            ref={fileInputRef}
            type="file"
            multiple
            accept="image/jpeg,image/png,image/webp,video/mp4,application/pdf"
            className="hidden"
            onChange={e => {
              setSelectedFiles(e.target.files);
              setErrors(previous => ({ ...previous, files: undefined, server: undefined }));
            }}
          />
          <button
            type="button"
            onClick={() => fileInputRef.current?.click()}
            className="inline-flex items-center gap-2 px-4 py-2 border border-border rounded-xl text-sm font-medium hover:bg-muted transition-colors"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" />
            </svg>
            {t('filesSelect')}
          </button>
          {selectedFiles && selectedFiles.length > 0 && (
            <p className="text-sm text-muted-foreground mt-2">
              {t('filesSelected', { count: selectedFiles.length })}
            </p>
          )}
          {errors.files && <p className={errorClass}>{errors.files}</p>}
        </div>
      </div>

      {/* Comment */}
      <div>
        <label className={labelClass}>{t('comment')}</label>
        <textarea
          value={form.customerComment}
          onChange={e => set('customerComment', e.target.value)}
          placeholder={t('commentPlaceholder')}
          rows={3}
          className={`${inputClass} resize-none`}
        />
      </div>

      {/* Consent */}
      <div>
        <label className={`flex items-start gap-3 cursor-pointer ${errors.consent ? 'text-red-600' : ''}`}>
          <input
            type="checkbox"
            checked={form.consent}
            onChange={e => set('consent', e.target.checked)}
            className="mt-0.5 accent-primary"
          />
          <span className="text-sm leading-relaxed">{t('consent')}</span>
        </label>
        {errors.consent && <p className={`${errorClass} ml-6`}>{errors.consent}</p>}
      </div>

      <button
        type="submit"
        disabled={submitting}
        className="w-full py-3 px-6 bg-primary text-primary-foreground rounded-xl font-semibold hover:bg-primary/90 disabled:opacity-60 disabled:cursor-not-allowed transition-colors"
      >
        {submitting ? t('submitting') : t('submit')}
      </button>
    </form>
  );
}
