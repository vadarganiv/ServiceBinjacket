'use client';

import Link from 'next/link';
import { useTranslations } from 'next-intl';
import type { ServiceListItem } from '@/lib/types';

interface Props {
  service: ServiceListItem;
  locale: string;
}

export default function ServiceCard({ service, locale }: Props) {
  const t = useTranslations('services');

  return (
    <Link
      href={`/${locale}/services/${service.slug}`}
      className="group flex flex-col bg-background border border-border rounded-xl overflow-hidden hover:shadow-lg hover:-translate-y-0.5 transition-all duration-200"
    >
      <div className="flex items-center justify-center h-32 bg-primary/5">
        <svg
          className="w-12 h-12 text-primary/40 group-hover:text-primary/60 transition-colors duration-200"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={1.5}
            d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"
          />
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
        </svg>
      </div>

      <div className="flex flex-col flex-1 p-4 gap-2">
        {service.category && (
          <span className="text-xs font-medium text-primary/70 uppercase tracking-wide">
            {service.category.name}
          </span>
        )}
        <h3 className="text-sm font-semibold leading-tight line-clamp-2 group-hover:text-primary transition-colors">
          {service.name}
        </h3>
        {service.shortDescription && (
          <p className="text-xs text-muted-foreground line-clamp-3">
            {service.shortDescription}
          </p>
        )}
        {service.priceNote && (
          <div className="mt-auto pt-2">
            <span className="text-xs font-medium text-muted-foreground">
              {t('priceNote')}: {service.priceNote}
            </span>
          </div>
        )}
      </div>
    </Link>
  );
}
