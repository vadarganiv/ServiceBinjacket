'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import type { ProductCategory, ProductCondition } from '@/lib/types';

const CONDITIONS: ProductCondition[] = ['New', 'Used', 'Refurbished', 'Unknown'];

interface FilterValues {
  categoryId: string;
  minPrice: string;
  maxPrice: string;
  condition: string;
  inStock: boolean;
}

interface Props {
  categories: ProductCategory[];
  values: FilterValues;
  onChange: (key: string, value: string | null) => void;
  onClear: () => void;
}

export default function ProductFilters({ categories, values, onChange, onClear }: Props) {
  const t = useTranslations('products');
  const [mobileOpen, setMobileOpen] = useState(false);

  const activeCount = [
    values.categoryId,
    values.minPrice,
    values.maxPrice,
    values.condition,
    values.inStock,
  ].filter(Boolean).length;

  const content = (
    <div className="flex flex-col gap-5">
      <div>
        <label className="text-xs font-semibold uppercase tracking-wide text-muted-foreground mb-2 block">
          {t('category')}
        </label>
        <select
          value={values.categoryId}
          onChange={e => onChange('category', e.target.value || null)}
          className="w-full text-sm border border-input rounded-lg px-3 py-2 bg-background focus:outline-none focus:ring-2 focus:ring-ring"
        >
          <option value="">{t('allCategories')}</option>
          {categories.map(cat => (
            <option key={cat.id} value={cat.id}>
              {cat.name}
            </option>
          ))}
        </select>
      </div>

      <div>
        <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground mb-2">
          {t('priceRange')}
        </p>
        <div className="flex gap-2">
          <input
            type="number"
            placeholder={t('minPrice')}
            value={values.minPrice}
            min={0}
            onChange={e => onChange('minPrice', e.target.value || null)}
            className="w-full text-sm border border-input rounded-lg px-3 py-2 bg-background focus:outline-none focus:ring-2 focus:ring-ring"
          />
          <input
            type="number"
            placeholder={t('maxPrice')}
            value={values.maxPrice}
            min={0}
            onChange={e => onChange('maxPrice', e.target.value || null)}
            className="w-full text-sm border border-input rounded-lg px-3 py-2 bg-background focus:outline-none focus:ring-2 focus:ring-ring"
          />
        </div>
      </div>

      <div>
        <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground mb-2">
          {t('condition')}
        </p>
        <div className="flex flex-wrap gap-2">
          {CONDITIONS.map(cond => (
            <button
              key={cond}
              type="button"
              onClick={() => onChange('condition', values.condition === cond ? null : cond)}
              className={`text-xs font-medium px-3 py-1.5 rounded-full border transition-colors ${
                values.condition === cond
                  ? 'bg-primary text-primary-foreground border-primary'
                  : 'bg-background border-border text-foreground/70 hover:border-primary/60'
              }`}
            >
              {t(`conditionBadge.${cond}` as 'conditionBadge.New')}
            </button>
          ))}
        </div>
      </div>

      <label className="flex items-center gap-2.5 cursor-pointer group select-none">
        <button
          type="button"
          role="switch"
          aria-checked={values.inStock}
          onClick={() => onChange('inStock', values.inStock ? null : 'true')}
          className={`w-10 h-6 rounded-full transition-colors flex items-center px-1 ${
            values.inStock ? 'bg-primary' : 'bg-muted'
          }`}
        >
          <div
            className={`w-4 h-4 rounded-full bg-white shadow transition-transform duration-200 ${
              values.inStock ? 'translate-x-4' : 'translate-x-0'
            }`}
          />
        </button>
        <span className="text-sm text-foreground/80 group-hover:text-foreground transition-colors">
          {t('inStockOnly')}
        </span>
      </label>

      {activeCount > 0 && (
        <button
          type="button"
          onClick={onClear}
          className="text-sm text-primary hover:underline text-left w-fit"
        >
          {t('clearFilters')}
        </button>
      )}
    </div>
  );

  return (
    <>
      {/* Mobile toggle */}
      <div className="lg:hidden w-full">
        <button
          type="button"
          onClick={() => setMobileOpen(o => !o)}
          className="flex items-center gap-2 text-sm font-medium border border-input rounded-xl px-4 py-2.5 bg-background hover:bg-muted transition-colors"
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 4h18M7 12h10m-3 8h-4" />
          </svg>
          {t('filters')}
          {activeCount > 0 && (
            <span className="bg-primary text-primary-foreground text-xs rounded-full w-5 h-5 flex items-center justify-center font-medium">
              {activeCount}
            </span>
          )}
        </button>
        {mobileOpen && (
          <div className="mt-3 border border-border rounded-xl p-5 bg-background">
            {content}
          </div>
        )}
      </div>

      {/* Desktop sidebar */}
      <aside className="hidden lg:block w-60 shrink-0">
        <div className="sticky top-24 border border-border rounded-xl p-5">
          <div className="flex items-center justify-between mb-5">
            <h2 className="text-sm font-semibold text-foreground">{t('filters')}</h2>
            {activeCount > 0 && (
              <span className="text-xs bg-primary text-primary-foreground rounded-full px-2 py-0.5 font-medium">
                {activeCount}
              </span>
            )}
          </div>
          {content}
        </div>
      </aside>
    </>
  );
}
