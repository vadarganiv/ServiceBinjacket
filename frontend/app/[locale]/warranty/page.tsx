import { getTranslations } from 'next-intl/server';
import type { Metadata } from 'next';

type Props = { params: Promise<{ locale: string }> };

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'warranty' });
  return {
    title: t('title'),
    description: t('metaDescription'),
    alternates: {
      canonical: `/${locale}/warranty`,
      languages: { sq: '/sq/warranty', en: '/en/warranty', 'x-default': '/sq/warranty' },
    },
  };
}

export default async function WarrantyPage({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'warranty' });

  return (
    <div className="container mx-auto px-4 py-12 max-w-3xl">
      <h1 className="text-3xl sm:text-4xl font-bold tracking-tight mb-8">{t('title')}</h1>

      <section className="border border-border rounded-xl p-6 mb-6">
        <h2 className="text-xl font-semibold mb-2">{t('productsTitle')}</h2>
        <p className="text-sm text-muted-foreground">{t('productsDesc')}</p>
      </section>

      <section className="border border-border rounded-xl p-6 mb-6">
        <h2 className="text-xl font-semibold mb-2">{t('repairsTitle')}</h2>
        <p className="text-sm text-muted-foreground">{t('repairsDesc')}</p>
      </section>

      <p className="text-sm text-muted-foreground italic">{t('todo')}</p>
    </div>
  );
}
