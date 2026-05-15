interface Props {
  status: string;
}

const ORDER_COLORS: Record<string, string> = {
  New: 'bg-blue-100 text-blue-800',
  Confirmed: 'bg-indigo-100 text-indigo-800',
  Preparing: 'bg-yellow-100 text-yellow-800',
  OutForDelivery: 'bg-orange-100 text-orange-800',
  SentByPost: 'bg-purple-100 text-purple-800',
  Delivered: 'bg-teal-100 text-teal-800',
  Completed: 'bg-green-100 text-green-800',
  Cancelled: 'bg-red-100 text-red-800',
};

const REPAIR_COLORS: Record<string, string> = {
  New: 'bg-blue-100 text-blue-800',
  Contacted: 'bg-indigo-100 text-indigo-800',
  WaitingForDevice: 'bg-yellow-100 text-yellow-800',
  Received: 'bg-teal-100 text-teal-800',
  Diagnostics: 'bg-purple-100 text-purple-800',
  PriceOffered: 'bg-orange-100 text-orange-800',
  PriceAgreed: 'bg-lime-100 text-lime-800',
  Repairing: 'bg-cyan-100 text-cyan-800',
  Ready: 'bg-green-100 text-green-800',
  SentBack: 'bg-gray-100 text-gray-800',
  Completed: 'bg-green-200 text-green-900',
  Cancelled: 'bg-red-100 text-red-800',
};

export default function StatusBadge({ status }: Props) {
  const color =
    ORDER_COLORS[status] ?? REPAIR_COLORS[status] ?? 'bg-gray-100 text-gray-700';
  return (
    <span className={`inline-flex px-2 py-0.5 rounded text-xs font-medium ${color}`}>
      {status}
    </span>
  );
}
