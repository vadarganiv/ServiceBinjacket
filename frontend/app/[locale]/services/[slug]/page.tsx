import { getTranslations } from 'next-intl/server';
import { notFound } from 'next/navigation';
import Link from 'next/link';
import type { Metadata } from 'next';
import { apiFetch } from '@/lib/api';
import type { ServiceDetail } from '@/lib/types';

type Props = {
  params: Promise<{ locale: string; slug: string }>;
};

async function fetchService(slug: string, locale: string): Promise<ServiceDetail | null> {
  try {
    return await apiFetch<ServiceDetail>(`/services/${slug}?locale=${locale}`);
  } catch {
    return null;
  }
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale, slug } = await params;
  const service = await fetchService(slug, locale);
  if (!service) return { title: 'Not found' };

  return {
    title: service.name,
    description: service.shortDescription ?? undefined,
    alternates: {
      canonical: `/${locale}/services/${slug}`,
      languages: {
        sq: `/sq/services/${slug}`,
        en: `/en/services/${slug}`,
        'x-default': `/sq/services/${slug}`,
      },
    },
    openGraph: {
      title: service.name,
      description: service.shortDescription ?? undefined,
      locale,
      type: 'website',
    },
  };
}

export default async function ServiceDetailPage({ params }: Props) {
  const { locale, slug } = await params;

  const [service, t] = await Promise.all([
    fetchService(slug, locale),
    getTranslations({ locale, namespace: 'serviceDetails' }),
  ]);

  if (!service) notFound();

  return (
    <div className="container mx-auto px-4 py-8">
      <Link
        href={`/${locale}/services`}
        className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground mb-6 transition-colors group"
      >
        <svg
          className="w-4 h-4 group-hover:-translate-x-0.5 transition-transform"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
        </svg>
        {t('backToServices')}
      </Link>

      <div className="max-w-3xl">
        {service.category && (
          <span className="text-sm font-medium text-primary/70 uppercase tracking-wider">
            {service.category.name}
          </span>
        )}
        <h1 className="text-3xl font-bold text-foreground mt-2 mb-4">{service.name}</h1>

        {service.shortDescription && (
          <p className="text-lg text-muted-foreground leading-relaxed mb-6">
            {service.shortDescription}
          </p>
        )}

        {service.priceNote && (
          <div className="inline-flex items-center gap-2 px-4 py-2 bg-primary/5 border border-primary/20 rounded-xl mb-6">
            <svg className="w-4 h-4 text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
            </svg>
            <span className="text-sm font-medium">
              {t('priceNote')}: {service.priceNote}
            </span>
          </div>
        )}

        {service.description && (
          <div className="mt-6 mb-8">
            <h2 className="text-xl font-semibold mb-3">{t('description')}</h2>
            <p className="text-muted-foreground leading-relaxed whitespace-pre-line">
              {service.description}
            </p>
          </div>
        )}

        <Link
          href={`/${locale}/repair?serviceId=${service.id}`}
          className="inline-flex items-center justify-center gap-2 px-6 py-3 bg-primary text-primary-foreground rounded-xl font-semibold hover:bg-primary/90 transition-colors"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
          </svg>
          {t('requestRepair')}
        </Link>
      </div>
    </div>
  );
}
