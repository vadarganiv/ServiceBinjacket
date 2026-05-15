import { Suspense } from 'react';
import { getTranslations } from 'next-intl/server';
import type { Metadata } from 'next';
import { apiFetch } from '@/lib/api';
import type { PagedResult, ProductCategory, ProductListItem } from '@/lib/types';
import ProductsClient from '@/features/products/ProductsClient';
import ProductCardSkeleton from '@/features/products/ProductCardSkeleton';

type SearchParams = {
  category?: string;
  q?: string;
  minPrice?: string;
  maxPrice?: string;
  condition?: string;
  inStock?: string;
  sort?: string;
  page?: string;
};

type Props = {
  params: Promise<{ locale: string }>;
  searchParams: Promise<SearchParams>;
};

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'products' });
  return {
    title: t('title'),
    description: t('metaDescription'),
    alternates: {
      canonical: `/${locale}/products`,
      languages: {
        sq: '/sq/products',
        en: '/en/products',
        'x-default': '/sq/products',
      },
    },
    openGraph: {
      title: t('title'),
      description: t('metaDescription'),
      locale,
    },
  };
}

async function fetchProducts(locale: string, sp: SearchParams) {
  const params = new URLSearchParams({ locale });
  if (sp.category) params.set('categoryId', sp.category);
  if (sp.q) params.set('q', sp.q);
  if (sp.minPrice) params.set('minPrice', sp.minPrice);
  if (sp.maxPrice) params.set('maxPrice', sp.maxPrice);
  if (sp.condition) params.set('condition', sp.condition);
  if (sp.inStock) params.set('inStock', sp.inStock);
  if (sp.sort) params.set('sort', sp.sort);
  params.set('page', sp.page ?? '1');
  params.set('pageSize', '24');
  return apiFetch<PagedResult<ProductListItem>>(`/products?${params.toString()}`);
}

async function fetchCategories(locale: string) {
  return apiFetch<ProductCategory[]>(`/product-categories?locale=${locale}`);
}

function GridSkeleton() {
  return (
    <div className="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-3 md:gap-4">
      {Array.from({ length: 8 }).map((_, i) => (
        <ProductCardSkeleton key={i} />
      ))}
    </div>
  );
}

export default async function ProductsPage({ params, searchParams }: Props) {
  const { locale } = await params;
  const sp = await searchParams;
  const t = await getTranslations({ locale, namespace: 'products' });

  const [data, categories] = await Promise.all([
    fetchProducts(locale, sp),
    fetchCategories(locale),
  ]);

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold tracking-tight mb-8">{t('title')}</h1>
      <Suspense fallback={<GridSkeleton />}>
        <ProductsClient locale={locale} data={data} categories={categories} />
      </Suspense>
    </div>
  );
}
