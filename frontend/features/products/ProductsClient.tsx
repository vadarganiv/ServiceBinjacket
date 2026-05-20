'use client';

import { Suspense, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { useRouter, usePathname, useSearchParams } from 'next/navigation';
import type { PagedResult, ProductCategory, ProductListItem, SortOption } from '@/lib/types';
import ProductCard from './ProductCard';
import ProductFilters from './ProductFilters';
import ProductCardSkeleton from './ProductCardSkeleton';

const SORT_OPTIONS: Array<{ value: SortOption; labelKey: string }> = [
  { value: 'newest', labelKey: 'sortNewest' },
  { value: 'price-asc', labelKey: 'sortPriceAsc' },
  { value: 'price-desc', labelKey: 'sortPriceDesc' },
  { value: 'name-asc', labelKey: 'sortNameAsc' },
];

interface Props {
  locale: string;
  data: PagedResult<ProductListItem>;
  categories: ProductCategory[];
}

function ProductsContent({ locale, data, categories }: Props) {
  const t = useTranslations('products');
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const sort = (searchParams.get('sort') as SortOption) ?? 'newest';
  const page = Number(searchParams.get('page') ?? '1');
  const q = searchParams.get('q') ?? '';
  const categoryId = searchParams.get('category') ?? '';
  const minPrice = searchParams.get('minPrice') ?? '';
  const maxPrice = searchParams.get('maxPrice') ?? '';
  const condition = searchParams.get('condition') ?? '';
  const inStock = searchParams.get('inStock') === 'true';

  function setParam(key: string, value: string | null) {
    const params = new URLSearchParams(searchParams.toString());
    if (value === null || value === '') {
      params.delete(key);
    } else {
      params.set(key, value);
    }
    if (key !== 'page') params.delete('page');
    router.push(`${pathname}?${params.toString()}`);
  }

  function handleSearch(value: string) {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => setParam('q', value || null), 400);
  }

  function clearFilters() {
    const params = new URLSearchParams();
    if (sort !== 'newest') params.set('sort', sort);
    if (q) params.set('q', q);
    const qs = params.toString();
    router.push(qs ? `${pathname}?${qs}` : pathname);
  }

  const totalPages = Math.ceil(data.total / (data.pageSize || 24));

  return (
    <div className="flex flex-col gap-5">
      {/* Search + Sort bar */}
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <svg
            className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground pointer-events-none"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M21 21l-4.35-4.35M11 19a8 8 0 100-16 8 8 0 000 16z"
            />
          </svg>
          <input
            type="search"
            defaultValue={q}
            placeholder={t('searchPlaceholder')}
            onChange={e => handleSearch(e.target.value)}
            className="w-full pl-9 pr-4 py-2.5 text-sm border border-input rounded-xl bg-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring"
          />
        </div>
        <select
          value={sort}
          onChange={e => setParam('sort', e.target.value)}
          className="text-sm border border-input rounded-xl px-3 py-2.5 bg-background focus:outline-none focus:ring-2 focus:ring-ring sm:w-52"
        >
          {SORT_OPTIONS.map(({ value, labelKey }) => (
            <option key={value} value={value}>
              {t(labelKey as 'sortNewest')}
            </option>
          ))}
        </select>
      </div>

      {/* Filters + grid layout */}
      <div className="flex flex-col lg:flex-row gap-6 items-start">
        <ProductFilters
          categories={categories}
          values={{ categoryId, minPrice, maxPrice, condition, inStock }}
          onChange={setParam}
          onClear={clearFilters}
        />

        {/* Grid area */}
        <div className="flex-1 min-w-0">
          <p className="text-sm text-muted-foreground mb-4">
            {data.total === 1
              ? t('result', { count: data.total })
              : t('results', { count: data.total })}
          </p>

          {data.items.length === 0 ? (
            <div className="flex flex-col items-center py-20 text-center">
              <svg
                className="w-14 h-14 text-muted-foreground/30 mb-4"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={1.5}
                  d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
              <p className="font-medium text-foreground">{t('noResults')}</p>
              <p className="text-sm text-muted-foreground mt-1">{t('noResultsHint')}</p>
            </div>
          ) : (
            <div className="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-3 md:gap-4">
              {data.items.map(product => (
                <ProductCard key={product.id} product={product} locale={locale} />
              ))}
            </div>
          )}

          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-3 mt-8">
              <button
                type="button"
                disabled={page <= 1}
                onClick={() => setParam('page', String(page - 1))}
                className="px-4 py-2 text-sm border border-input rounded-xl bg-background hover:bg-muted disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
              >
                {t('prevPage')}
              </button>
              <span className="text-sm text-muted-foreground">
                {t('page')} {page} / {totalPages}
              </span>
              <button
                type="button"
                disabled={page >= totalPages}
                onClick={() => setParam('page', String(page + 1))}
                className="px-4 py-2 text-sm border border-input rounded-xl bg-background hover:bg-muted disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
              >
                {t('nextPage')}
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function SkeletonFallback() {
  return (
    <div className="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-3 md:gap-4">
      {Array.from({ length: 8 }).map((_, i) => (
        <ProductCardSkeleton key={i} />
      ))}
    </div>
  );
}

export default function ProductsClient(props: Props) {
  return (
    <Suspense fallback={<SkeletonFallback />}>
      <ProductsContent {...props} />
    </Suspense>
  );
}
