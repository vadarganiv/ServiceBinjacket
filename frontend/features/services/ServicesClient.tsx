'use client';

import { Suspense } from 'react';
import { useTranslations } from 'next-intl';
import { useRouter, usePathname, useSearchParams } from 'next/navigation';
import type { ServiceCategory, ServiceListItem } from '@/lib/types';
import ServiceCard from './ServiceCard';
import ServiceCardSkeleton from './ServiceCardSkeleton';

interface Props {
  locale: string;
  services: ServiceListItem[];
  categories: ServiceCategory[];
}

function ServicesContent({ locale, services, categories }: Props) {
  const t = useTranslations('services');
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const selectedCategory = searchParams.get('category') ?? '';

  function setCategory(categoryId: string) {
    const params = new URLSearchParams(searchParams.toString());
    if (categoryId) {
      params.set('category', categoryId);
    } else {
      params.delete('category');
    }
    router.push(`${pathname}?${params.toString()}`);
  }

  return (
    <div className="flex flex-col gap-6">
      {/* Category filter tabs */}
      {categories.length > 0 && (
        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            onClick={() => setCategory('')}
            className={`px-4 py-2 text-sm rounded-full border transition-colors ${
              !selectedCategory
                ? 'bg-primary text-primary-foreground border-primary'
                : 'border-border bg-background text-foreground/70 hover:text-foreground hover:border-foreground/30'
            }`}
          >
            {t('allCategories')}
          </button>
          {categories.map(cat => (
            <button
              key={cat.id}
              type="button"
              onClick={() => setCategory(String(cat.id))}
              className={`px-4 py-2 text-sm rounded-full border transition-colors ${
                selectedCategory === String(cat.id)
                  ? 'bg-primary text-primary-foreground border-primary'
                  : 'border-border bg-background text-foreground/70 hover:text-foreground hover:border-foreground/30'
              }`}
            >
              {cat.name}
            </button>
          ))}
        </div>
      )}

      {/* Grid */}
      {services.length === 0 ? (
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
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
          {services.map(service => (
            <ServiceCard key={service.id} service={service} locale={locale} />
          ))}
        </div>
      )}
    </div>
  );
}

function SkeletonFallback() {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
      {Array.from({ length: 8 }).map((_, i) => (
        <ServiceCardSkeleton key={i} />
      ))}
    </div>
  );
}

export default function ServicesClient(props: Props) {
  return (
    <Suspense fallback={<SkeletonFallback />}>
      <ServicesContent {...props} />
    </Suspense>
  );
}
