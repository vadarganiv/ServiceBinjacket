import { getTranslations } from 'next-intl/server';
import type { Metadata } from 'next';

type Props = { params: Promise<{ locale: string }> };

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'payment' });
  return {
    title: t('title'),
    description: t('metaDescription'),
    alternates: {
      canonical: `/${locale}/payment`,
      languages: { sq: '/sq/payment', en: '/en/payment', 'x-default': '/sq/payment' },
    },
  };
}

export default async function PaymentPage({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'payment' });

  return (
    <div className="container mx-auto px-4 py-12 max-w-3xl">
      <h1 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4">{t('title')}</h1>
      <p className="text-muted-foreground mb-10">{t('intro')}</p>

      <section className="border border-border rounded-xl p-6 mb-6">
        <h2 className="text-xl font-semibold mb-2">{t('cashOnDeliveryTitle')}</h2>
        <p className="text-sm text-muted-foreground">{t('cashOnDeliveryDesc')}</p>
      </section>

      <section className="border border-border rounded-xl p-6 mb-6">
        <h2 className="text-xl font-semibold mb-2">{t('cashInStoreTitle')}</h2>
        <p className="text-sm text-muted-foreground">{t('cashInStoreDesc')}</p>
      </section>

      <p className="text-sm text-muted-foreground italic">{t('noCardsNote')}</p>
    </div>
  );
}
