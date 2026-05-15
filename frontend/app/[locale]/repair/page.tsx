import { getTranslations } from 'next-intl/server';
import type { Metadata } from 'next';
import { apiFetch } from '@/lib/api';
import type { ServiceListItem } from '@/lib/types';
import RepairRequestForm from '@/features/repair/RepairRequestForm';

type Props = {
  params: Promise<{ locale: string }>;
};

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'repairRequest' });
  return {
    title: t('title'),
    description: t('metaDescription'),
    alternates: {
      canonical: `/${locale}/repair`,
      languages: {
        sq: '/sq/repair',
        en: '/en/repair',
        'x-default': '/sq/repair',
      },
    },
    openGraph: {
      title: t('title'),
      description: t('metaDescription'),
      locale,
    },
  };
}

async function fetchServices(locale: string) {
  try {
    return await apiFetch<ServiceListItem[]>(`/services?locale=${locale}`);
  } catch {
    return [];
  }
}

export default async function RepairPage({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'repairRequest' });

  const services = await fetchServices(locale);

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-2xl mx-auto mb-10">
        <h1 className="text-3xl font-bold tracking-tight mb-2">{t('title')}</h1>
        <p className="text-muted-foreground">{t('subtitle')}</p>
      </div>
      <RepairRequestForm locale={locale} services={services} />
    </div>
  );
}
