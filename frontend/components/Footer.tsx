import Link from 'next/link';
import { getTranslations } from 'next-intl/server';

interface Props {
  locale: string;
}

export default async function Footer({ locale }: Props) {
  const t = await getTranslations({ locale, namespace: 'footer' });
  const tNav = await getTranslations({ locale, namespace: 'nav' });

  const address = process.env.PUBLIC_ADDRESS ?? '';
  const phone = process.env.PUBLIC_PHONE ?? '';
  const hours = process.env.PUBLIC_HOURS ?? '';
  const whatsappPhone = process.env.WHATSAPP_PHONE ?? '';
  const waDigits = whatsappPhone.replace(/\D/g, '');
  const waUrl = waDigits ? `https://wa.me/${waDigits}` : '';
  const telUrl = phone ? `tel:${phone.replace(/\s/g, '')}` : '';

  const year = new Date().getFullYear();

  return (
    <footer className="border-t border-border bg-muted/30 mt-12">
      <div className="container mx-auto px-4 py-10">
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-8">
          {/* Shop */}
          <div>
            <h3 className="text-sm font-semibold mb-3">{t('shopColumn')}</h3>
            <ul className="space-y-2 text-sm">
              <li>
                <Link href={`/${locale}/products`} className="text-muted-foreground hover:text-foreground">
                  {tNav('products')}
                </Link>
              </li>
              <li>
                <Link href={`/${locale}/services`} className="text-muted-foreground hover:text-foreground">
                  {tNav('services')}
                </Link>
              </li>
              <li>
                <Link href={`/${locale}/repair`} className="text-muted-foreground hover:text-foreground">
                  {tNav('repair')}
                </Link>
              </li>
            </ul>
          </div>

          {/* Information */}
          <div>
            <h3 className="text-sm font-semibold mb-3">{t('infoColumn')}</h3>
            <ul className="space-y-2 text-sm">
              <li>
                <Link href={`/${locale}/delivery`} className="text-muted-foreground hover:text-foreground">
                  {t('delivery')}
                </Link>
              </li>
              <li>
                <Link href={`/${locale}/payment`} className="text-muted-foreground hover:text-foreground">
                  {t('payment')}
                </Link>
              </li>
              <li>
                <Link href={`/${locale}/warranty`} className="text-muted-foreground hover:text-foreground">
                  {t('warranty')}
                </Link>
              </li>
              <li>
                <Link href={`/${locale}/contact`} className="text-muted-foreground hover:text-foreground">
                  {t('contact')}
                </Link>
              </li>
            </ul>
          </div>

          {/* Contacts */}
          <div className="col-span-2 sm:col-span-1">
            <h3 className="text-sm font-semibold mb-3">{t('contactsColumn')}</h3>
            <ul className="space-y-2 text-sm text-muted-foreground">
              {address && <li>{address}</li>}
              {phone && (
                <li>
                  <a href={telUrl} className="hover:text-foreground">{phone}</a>
                </li>
              )}
              {hours && <li className="whitespace-pre-line">{hours}</li>}
              {waUrl && (
                <li>
                  <a
                    href={waUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="inline-flex items-center gap-1.5 text-green-600 hover:text-green-700 font-medium"
                  >
                    <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
                      <path d="M11.99 0C5.375 0 0 5.373 0 12c0 2.117.553 4.103 1.518 5.829L0 24l6.335-1.652A11.954 11.954 0 0011.99 24C18.614 24 24 18.627 24 12S18.614 0 11.99 0zm.01 21.818a9.817 9.817 0 01-5.006-1.362l-.36-.214-3.732.979 1.001-3.648-.235-.374A9.818 9.818 0 012.182 12c0-5.42 4.41-9.818 9.818-9.818 5.42 0 9.818 4.41 9.818 9.818 0 5.42-4.398 9.818-9.818 9.818z" />
                    </svg>
                    {t('whatsappLabel')}
                  </a>
                </li>
              )}
            </ul>
          </div>

          {/* Languages */}
          <div className="col-span-2 sm:col-span-1">
            <h3 className="text-sm font-semibold mb-3">{t('languagesColumn')}</h3>
            <ul className="space-y-2 text-sm">
              <li>
                <Link href="/sq" className="text-muted-foreground hover:text-foreground">
                  {t('languageSq')}
                </Link>
              </li>
              <li>
                <Link href="/en" className="text-muted-foreground hover:text-foreground">
                  {t('languageEn')}
                </Link>
              </li>
            </ul>
          </div>
        </div>

        <div className="mt-8 pt-6 border-t border-border text-xs text-muted-foreground text-center">
          {t('copyright', { year })}
        </div>
      </div>
    </footer>
  );
}
