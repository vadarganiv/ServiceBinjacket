import ServiceCardSkeleton from '@/features/services/ServiceCardSkeleton';

export default function ServicesLoading() {
  return (
    <div className="container mx-auto px-4 py-8">
      <div className="h-9 w-48 bg-muted rounded animate-pulse mb-8" />
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
        {Array.from({ length: 8 }).map((_, i) => (
          <ServiceCardSkeleton key={i} />
        ))}
      </div>
    </div>
  );
}
