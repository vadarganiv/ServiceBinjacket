import { getTranslations } from 'next-intl/server';
import type { Metadata } from 'next';

type Props = { params: Promise<{ locale: string }> };

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'delivery' });
  return {
    title: t('title'),
    description: t('metaDescription'),
    alternates: {
      canonical: `/${locale}/delivery`,
      languages: { sq: '/sq/delivery', en: '/en/delivery', 'x-default': '/sq/delivery' },
    },
  };
}

export default async function DeliveryPage({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'delivery' });

  return (
    <div className="container mx-auto px-4 py-12 max-w-3xl">
      <h1 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4">{t('title')}</h1>
      <p className="text-muted-foreground mb-10">{t('intro')}</p>

      <section className="border border-border rounded-xl p-6 mb-6">
        <h2 className="text-xl font-semibold mb-3">{t('durresTitle')}</h2>
        <ul className="space-y-2 text-sm">
          <li className="flex gap-2"><span className="text-primary">•</span>{t('durresStorePickup')}</li>
          <li className="flex gap-2"><span className="text-primary">•</span>{t('durresLocalCourier')}</li>
        </ul>
      </section>

      <section className="border border-border rounded-xl p-6 mb-6">
        <h2 className="text-xl font-semibold mb-3">{t('outsideTitle')}</h2>
        <ul className="space-y-2 text-sm">
          <li className="flex gap-2"><span className="text-primary">•</span>{t('outsidePostal')}</li>
          <li className="flex gap-2"><span className="text-primary">•</span>{t('outsideManual')}</li>
        </ul>
      </section>

      <p className="text-sm text-muted-foreground">{t('note')}</p>
    </div>
  );
}
