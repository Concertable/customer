import {
  ConcertHeaderCarousel,
  ArtistHeaderCarousel,
  VenueHeaderCarousel,
} from "@/features/search";

export function CustomerHomePage() {
  return (
    <div className="mx-auto max-w-6xl space-y-10 px-6 py-10">
      <ConcertHeaderCarousel title="Discover Concerts" />
      <div className="border-border border-t" />
      <ArtistHeaderCarousel title="Discover Artists" />
      <div className="border-border border-t" />
      <VenueHeaderCarousel title="Discover Venues" />
    </div>
  );
}
