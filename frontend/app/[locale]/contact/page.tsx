import { getTranslations } from 'next-intl/server';
import type { Metadata } from 'next';

type Props = { params: Promise<{ locale: string }> };

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'contact' });
  return {
    title: t('title'),
    description: t('metaDescription'),
    alternates: {
      canonical: `/${locale}/contact`,
      languages: { sq: '/sq/contact', en: '/en/contact', 'x-default': '/sq/contact' },
    },
  };
}

export default async function ContactPage({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'contact' });

  const address = process.env.PUBLIC_ADDRESS ?? '';
  const phone = process.env.PUBLIC_PHONE ?? '';
  const hours = process.env.PUBLIC_HOURS ?? '';
  const whatsappPhone = process.env.WHATSAPP_PHONE ?? '';

  const waDigits = whatsappPhone.replace(/\D/g, '');
  const waText = encodeURIComponent(
    locale === 'sq'
      ? 'Përshëndetje! Kam një pyetje për Servis Binjaket.'
      : 'Hello! I have a question for Servis Binjaket.'
  );
  const waUrl = waDigits ? `https://wa.me/${waDigits}?text=${waText}` : '';
  const telUrl = phone ? `tel:${phone.replace(/\s/g, '')}` : '';

  return (
    <div className="container mx-auto px-4 py-12 max-w-3xl">
      <h1 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4">{t('title')}</h1>
      <p className="text-muted-foreground mb-10">{t('intro')}</p>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-8">
        <section className="border border-border rounded-xl p-6">
          <h2 className="text-sm font-medium text-muted-foreground mb-2">{t('addressTitle')}</h2>
          <p className="text-base">{address || t('addressTodo')}</p>
        </section>

        <section className="border border-border rounded-xl p-6">
          <h2 className="text-sm font-medium text-muted-foreground mb-2">{t('phoneTitle')}</h2>
          {phone ? (
            <a href={telUrl} className="text-base text-primary hover:underline">{phone}</a>
          ) : (
            <p className="text-base">{t('phoneTodo')}</p>
          )}
        </section>

        <section className="border border-border rounded-xl p-6">
          <h2 className="text-sm font-medium text-muted-foreground mb-2">{t('hoursTitle')}</h2>
          <p className="text-base whitespace-pre-line">{hours || t('hoursTodo')}</p>
        </section>

        <section className="border border-border rounded-xl p-6">
          <h2 className="text-sm font-medium text-muted-foreground mb-2">{t('whatsappTitle')}</h2>
          {waUrl ? (
            <a
              href={waUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-2 text-base text-primary hover:underline"
            >
              {t('whatsappCta')} →
            </a>
          ) : (
            <p className="text-base text-muted-foreground">—</p>
          )}
        </section>
      </div>

      <div className="flex flex-col sm:flex-row gap-3 sm:gap-4">
        {waUrl && (
          <a
            href={waUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center justify-center gap-2 px-6 py-3 rounded-xl bg-green-500 hover:bg-green-600 text-white font-semibold transition-colors"
          >
            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
              <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347z" />
              <path d="M11.99 0C5.375 0 0 5.373 0 12c0 2.117.553 4.103 1.518 5.829L0 24l6.335-1.652A11.954 11.954 0 0011.99 24C18.614 24 24 18.627 24 12S18.614 0 11.99 0zm.01 21.818a9.817 9.817 0 01-5.006-1.362l-.36-.214-3.732.979 1.001-3.648-.235-.374A9.818 9.818 0 012.182 12c0-5.42 4.41-9.818 9.818-9.818 5.42 0 9.818 4.41 9.818 9.818 0 5.42-4.398 9.818-9.818 9.818z" />
            </svg>
            {t('whatsappCta')}
          </a>
        )}
        {telUrl && (
          <a
            href={telUrl}
            className="inline-flex items-center justify-center gap-2 px-6 py-3 rounded-xl border border-border bg-background text-foreground font-semibold hover:bg-muted transition-colors"
          >
            {t('callCta')}
          </a>
        )}
      </div>
    </div>
  );
}
