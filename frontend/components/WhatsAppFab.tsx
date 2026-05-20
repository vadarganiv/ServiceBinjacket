interface Props {
  phone: string;
  locale: string;
}

export default function WhatsAppFab({ phone, locale }: Props) {
  const digits = phone.replace(/\D/g, '');
  if (!digits) return null;

  const text = encodeURIComponent(
    locale === 'sq'
      ? 'Përshëndetje! Kam një pyetje për Servis Binjaket.'
      : 'Hello! I have a question for Servis Binjaket.'
  );
  const href = `https://wa.me/${digits}?text=${text}`;

  const label = locale === 'sq' ? 'Shkruaj në WhatsApp' : 'Message on WhatsApp';

  return (
    <a
      href={href}
      target="_blank"
      rel="noopener noreferrer"
      aria-label={label}
      className="fixed bottom-5 right-5 z-50 w-14 h-14 rounded-full bg-green-500 hover:bg-green-600 text-white shadow-lg hover:shadow-xl flex items-center justify-center transition-all hover:scale-105"
    >
      <svg className="w-7 h-7" fill="currentColor" viewBox="0 0 24 24">
        <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347z" />
        <path d="M11.99 0C5.375 0 0 5.373 0 12c0 2.117.553 4.103 1.518 5.829L0 24l6.335-1.652A11.954 11.954 0 0011.99 24C18.614 24 24 18.627 24 12S18.614 0 11.99 0zm.01 21.818a9.817 9.817 0 01-5.006-1.362l-.36-.214-3.732.979 1.001-3.648-.235-.374A9.818 9.818 0 012.182 12c0-5.42 4.41-9.818 9.818-9.818 5.42 0 9.818 4.41 9.818 9.818 0 5.42-4.398 9.818-9.818 9.818z" />
      </svg>
    </a>
  );
}
