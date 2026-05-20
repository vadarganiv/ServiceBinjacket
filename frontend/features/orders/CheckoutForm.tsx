'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { useCart } from '@/features/cart/CartContext';
import type { DeliveryMethod, PaymentMethod, OrderResponse } from '@/lib/types';

interface Props {
  locale: string;
}

interface FormErrors {
  fullName?: string;
  phone?: string;
  city?: string;
  address?: string;
  consent?: string;
  server?: string;
}

const DELIVERY_METHODS: DeliveryMethod[] = ['StorePickup', 'LocalCourier', 'PostalShipping', 'ManualAgreement'];
const PAYMENT_METHODS: PaymentMethod[] = ['CashOnDelivery', 'CashInStore'];

export default function CheckoutForm({ locale }: Props) {
  const t = useTranslations('checkout');
  const tSuccess = useTranslations('success');
  const tProducts = useTranslations('products');
  const { items, subtotal, clearCart } = useCart();

  const [submitting, setSubmitting] = useState(false);
  const [errors, setErrors] = useState<FormErrors>({});
  const [successId, setSuccessId] = useState<number | null>(null);

  const [form, setForm] = useState({
    fullName: '',
    phone: '',
    whatsAppPhone: '',
    city: '',
    address: '',
    deliveryMethod: 'StorePickup' as DeliveryMethod,
    paymentMethod: 'CashOnDelivery' as PaymentMethod,
    customerComment: '',
    consent: false,
  });

  function set(field: string, value: string | boolean) {
    setForm(prev => ({ ...prev, [field]: value }));
    setErrors(prev => ({ ...prev, [field]: undefined, server: undefined }));
  }

  const addressRequired = form.deliveryMethod !== 'StorePickup';

  function validate(): FormErrors {
    const errs: FormErrors = {};
    if (!form.fullName.trim()) errs.fullName = t('errorRequired');
    if (!form.phone.trim()) errs.phone = t('errorRequired');
    else if (!/^\+?[0-9\s\-()]{7,20}$/.test(form.phone.trim())) errs.phone = t('errorPhone');
    if (!form.city.trim()) errs.city = t('errorRequired');
    if (addressRequired && !form.address.trim()) errs.address = t('errorAddressRequired');
    if (!form.consent) errs.consent = t('errorConsent');
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
      const res = await fetch('/api/v1/orders', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          customer: {
            fullName: form.fullName.trim(),
            phone: form.phone.trim(),
            whatsAppPhone: form.whatsAppPhone.trim() || undefined,
            city: form.city.trim(),
            address: form.address.trim() || undefined,
          },
          deliveryMethod: form.deliveryMethod,
          paymentMethod: form.paymentMethod,
          items: items.map(i => ({ productId: i.productId, quantity: i.quantity })),
          customerComment: form.customerComment.trim() || undefined,
          consent: form.consent,
        }),
      });

      if (!res.ok) {
        const data = await res.json().catch(() => null);
        setErrors({ server: data?.error?.message ?? t('errorRequired') });
        return;
      }

      const created: OrderResponse = await res.json();
      clearCart();
      setSuccessId(created.id);
    } catch {
      setErrors({ server: 'Network error. Please try again.' });
    } finally {
      setSubmitting(false);
    }
  }

  if (items.length === 0 && successId === null) {
    return (
      <div className="flex flex-col items-center text-center py-20 px-4">
        <div className="w-16 h-16 rounded-full bg-muted flex items-center justify-center mb-6">
          <svg className="w-8 h-8 text-muted-foreground" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
          </svg>
        </div>
        <h2 className="text-xl font-bold mb-2">{t('emptyCart')}</h2>
        <p className="text-muted-foreground mb-6">{t('emptyCartHint')}</p>
        <Link href={`/${locale}/products`} className="inline-flex items-center justify-center px-6 py-3 bg-primary text-primary-foreground rounded-xl font-semibold hover:bg-primary/90 transition-colors">
          {t('goToProducts')}
        </Link>
      </div>
    );
  }

  if (successId !== null) {
    return (
      <div className="flex flex-col items-center text-center py-16 px-4 max-w-md mx-auto">
        <div className="w-16 h-16 rounded-full bg-emerald-100 flex items-center justify-center mb-6">
          <svg className="w-8 h-8 text-emerald-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
          </svg>
        </div>
        <h2 className="text-2xl font-bold text-foreground mb-3">{tSuccess('orderTitle')}</h2>
        <p className="text-muted-foreground mb-2">{tSuccess('orderMessage')}</p>
        <p className="text-sm font-medium text-primary mb-8">
          {tSuccess('orderId').replace('{id}', String(successId))}
        </p>
        <Link href={`/${locale}`} className="inline-flex items-center justify-center px-6 py-3 bg-primary text-primary-foreground rounded-xl font-semibold hover:bg-primary/90 transition-colors">
          {tSuccess('backToHome')}
        </Link>
      </div>
    );
  }

  const inputClass = "w-full px-4 py-2.5 text-sm border border-input rounded-xl bg-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring";
  const errorClass = "text-xs text-red-500 mt-1";
  const labelClass = "block text-sm font-medium text-foreground mb-1.5";

  return (
    <div className="max-w-4xl mx-auto grid grid-cols-1 lg:grid-cols-[1fr_360px] gap-8">
      {/* Form */}
      <form onSubmit={handleSubmit} className="space-y-8" noValidate>
        {errors.server && (
          <div className="rounded-xl bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">
            {errors.server}
          </div>
        )}

        {/* Customer */}
        <div className="space-y-4">
          <h2 className="text-lg font-semibold border-b border-border pb-2">{t('customerSection')}</h2>
          <div>
            <label className={labelClass}>{t('fullName')} *</label>
            <input type="text" value={form.fullName} onChange={e => set('fullName', e.target.value)}
              placeholder={t('fullNamePlaceholder')} className={`${inputClass} ${errors.fullName ? 'border-red-400' : ''}`} />
            {errors.fullName && <p className={errorClass}>{errors.fullName}</p>}
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className={labelClass}>{t('phone')} *</label>
              <input type="tel" value={form.phone} onChange={e => set('phone', e.target.value)}
                placeholder={t('phonePlaceholder')} className={`${inputClass} ${errors.phone ? 'border-red-400' : ''}`} />
              {errors.phone && <p className={errorClass}>{errors.phone}</p>}
            </div>
            <div>
              <label className={labelClass}>{t('whatsApp')}</label>
              <input type="tel" value={form.whatsAppPhone} onChange={e => set('whatsAppPhone', e.target.value)}
                placeholder={t('whatsAppPlaceholder')} className={inputClass} />
            </div>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className={labelClass}>{t('city')} *</label>
              <input type="text" value={form.city} onChange={e => set('city', e.target.value)}
                placeholder={t('cityPlaceholder')} className={`${inputClass} ${errors.city ? 'border-red-400' : ''}`} />
              {errors.city && <p className={errorClass}>{errors.city}</p>}
            </div>
            <div>
              <label className={labelClass}>{t('address')}{addressRequired ? ' *' : ''}</label>
              <input type="text" value={form.address} onChange={e => set('address', e.target.value)}
                placeholder={t('addressPlaceholder')} className={`${inputClass} ${errors.address ? 'border-red-400' : ''}`} />
              {errors.address && <p className={errorClass}>{errors.address}</p>}
            </div>
          </div>
        </div>

        {/* Delivery */}
        <div className="space-y-3">
          <h2 className="text-lg font-semibold border-b border-border pb-2">{t('deliverySection')}</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
            {DELIVERY_METHODS.map(method => (
              <label key={method} className={`flex items-center gap-2.5 px-4 py-3 border rounded-xl cursor-pointer transition-colors ${
                form.deliveryMethod === method ? 'border-primary bg-primary/5' : 'border-border hover:border-foreground/30'
              }`}>
                <input type="radio" name="delivery" value={method} checked={form.deliveryMethod === method}
                  onChange={() => set('deliveryMethod', method)} className="accent-primary" />
                <span className="text-sm">{t(`delivery${method}` as 'deliveryStorePickup')}</span>
              </label>
            ))}
          </div>
        </div>

        {/* Payment */}
        <div className="space-y-3">
          <h2 className="text-lg font-semibold border-b border-border pb-2">{t('paymentSection')}</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
            {PAYMENT_METHODS.map(method => (
              <label key={method} className={`flex items-center gap-2.5 px-4 py-3 border rounded-xl cursor-pointer transition-colors ${
                form.paymentMethod === method ? 'border-primary bg-primary/5' : 'border-border hover:border-foreground/30'
              }`}>
                <input type="radio" name="payment" value={method} checked={form.paymentMethod === method}
                  onChange={() => set('paymentMethod', method)} className="accent-primary" />
                <span className="text-sm">{t(`payment${method}` as 'paymentCashOnDelivery')}</span>
              </label>
            ))}
          </div>
        </div>

        {/* Comment */}
        <div>
          <label className={labelClass}>{t('comment')}</label>
          <textarea value={form.customerComment} onChange={e => set('customerComment', e.target.value)}
            placeholder={t('commentPlaceholder')} rows={3} className={`${inputClass} resize-none`} />
        </div>

        {/* Consent */}
        <div>
          <label className={`flex items-start gap-3 cursor-pointer ${errors.consent ? 'text-red-600' : ''}`}>
            <input type="checkbox" checked={form.consent} onChange={e => set('consent', e.target.checked)}
              className="mt-0.5 accent-primary" />
            <span className="text-sm leading-relaxed">{t('consent')}</span>
          </label>
          {errors.consent && <p className={`${errorClass} ml-6`}>{errors.consent}</p>}
        </div>

        <button type="submit" disabled={submitting}
          className="w-full py-3 px-6 bg-primary text-primary-foreground rounded-xl font-semibold hover:bg-primary/90 disabled:opacity-60 disabled:cursor-not-allowed transition-colors">
          {submitting ? t('submitting') : t('submit')}
        </button>
      </form>

      {/* Order summary */}
      <div className="lg:sticky lg:top-24 h-fit">
        <div className="border border-border rounded-2xl p-6 space-y-4">
          <h2 className="text-lg font-semibold">{t('orderSummary')}</h2>
          <div className="space-y-3">
            {items.map(item => (
              <div key={item.productId} className="flex gap-3">
                <div className="w-12 h-12 flex-shrink-0 rounded-lg bg-muted overflow-hidden">
                  {item.image
                    ? <img src={item.image.path} alt={item.image.alt} className="w-full h-full object-cover" /> // eslint-disable-line @next/next/no-img-element
                    : <div className="w-full h-full bg-muted" />}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium line-clamp-2">{item.name}</p>
                  <p className="text-xs text-muted-foreground">× {item.quantity}</p>
                </div>
                <p className="text-sm font-semibold whitespace-nowrap">
                  {(item.price * item.quantity).toLocaleString()} {tProducts('currency')}
                </p>
              </div>
            ))}
          </div>
          <div className="border-t border-border pt-4 flex items-center justify-between">
            <span className="font-semibold">{t('total')}</span>
            <span className="text-xl font-bold text-primary">
              {subtotal.toLocaleString()} {tProducts('currency')}
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
