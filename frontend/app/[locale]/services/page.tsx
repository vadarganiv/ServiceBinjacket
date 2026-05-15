import { Suspense } from 'react';
import { getTranslations } from 'next-intl/server';
import type { Metadata } from 'next';
import { apiFetch } from '@/lib/api';
import type { ServiceCategory, ServiceListItem } from '@/lib/types';
import ServicesClient from '@/features/services/ServicesClient';
import ServiceCardSkeleton from '@/features/services/ServiceCardSkeleton';

type SearchParams = {
  category?: string;
};

type Props = {
  params: Promise<{ locale: string }>;
  searchParams: Promise<SearchParams>;
};

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'services' });
  return {
    title: t('title'),
    description: t('metaDescription'),
    alternates: {
      canonical: `/${locale}/services`,
      languages: {
        sq: '/sq/services',
        en: '/en/services',
        'x-default': '/sq/services',
      },
    },
    openGraph: {
      title: t('title'),
      description: t('metaDescription'),
      locale,
    },
  };
}

function GridSkeleton() {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
      {Array.from({ length: 8 }).map((_, i) => (
        <ServiceCardSkeleton key={i} />
      ))}
    </div>
  );
}

async function fetchServices(locale: string, categoryId?: string) {
  const params = new URLSearchParams({ locale });
  if (categoryId) params.set('categoryId', categoryId);
  return apiFetch<ServiceListItem[]>(`/services?${params.toString()}`);
}

async function fetchCategories(locale: string) {
  return apiFetch<ServiceCategory[]>(`/service-categories?locale=${locale}`);
}

export default async function ServicesPage({ params, searchParams }: Props) {
  const { locale } = await params;
  const sp = await searchParams;
  const t = await getTranslations({ locale, namespace: 'services' });

  const [services, categories] = await Promise.all([
    fetchServices(locale, sp.category),
    fetchCategories(locale),
  ]);

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold tracking-tight mb-8">{t('title')}</h1>
      <Suspense fallback={<GridSkeleton />}>
        <ServicesClient locale={locale} services={services} categories={categories} />
      </Suspense>
    </div>
  );
}
