import { ScrollView, View, Pressable, RefreshControl } from "react-native";
import { useNavigation } from "@react-navigation/native";
import type { NativeStackNavigationProp } from "@react-navigation/native-stack";
import type { BottomTabNavigationProp } from "@react-navigation/bottom-tabs";
import { useHeaderAmountQuery } from "@concertable/shared/features/search";
import type { ConcertHeader, ArtistHeader, VenueHeader } from "@concertable/shared/features/search";
import { useImageUrl } from "@concertable/shared/hooks";
import { useSearchFiltersStore } from "@concertable/shared/features/search";
import { Image } from "expo-image";
import { MapPin } from "lucide-react-native";
import { Screen } from "@/components/ui/Screen";
import { Navbar } from "@/components/ui/Navbar";
import { RatingStars } from "@/components/ui/RatingStars";
import { GenreChips } from "@/components/ui/GenreChips";
import { EmptyState } from "@/components/ui/EmptyState";
import { Skeleton } from "@/components/ui/skeleton";
import { Text } from "@/components/ui/text";
import { theme } from "shared/lib/theme";
import type { HomeStackParamList, CustomerTabParamList } from "../../../navigation/types";
import dayjs from "dayjs";

type HomeNav = NativeStackNavigationProp<HomeStackParamList>;
type TabNav = BottomTabNavigationProp<CustomerTabParamList>;

export function HomeScreen() {
  const homeNav = useNavigation<HomeNav>();
  const tabNav = useNavigation<TabNav>();
  const { setFilters } = useSearchFiltersStore();

  const { data: concerts, isLoading: concertsLoading, refetch: refetchConcerts } = useHeaderAmountQuery("concert", 15);
  const { data: artists, isLoading: artistsLoading, refetch: refetchArtists } = useHeaderAmountQuery("artist", 15);
  const { data: venues, isLoading: venuesLoading, refetch: refetchVenues } = useHeaderAmountQuery("venue", 15);

  const refreshing = concertsLoading || artistsLoading || venuesLoading;

  function onRefresh() {
    refetchConcerts();
    refetchArtists();
    refetchVenues();
  }

  function goToSearch(headerType: "concert" | "artist" | "venue") {
    setFilters({ headerType });
    tabNav.navigate("SearchTab", { screen: "SearchMain" });
  }

  return (
    <Screen padded={false} header={<Navbar />}>

      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor={theme.primary} />}
        contentContainerClassName="pb-6"
      >
        <Section
          title="Discover Concerts"
          onSeeAll={() => goToSearch("concert")}
          loading={concertsLoading}
        >
          {concerts?.map((item) => (
            <ConcertCard
              key={item.id}
              concert={item as ConcertHeader}
              onPress={() => homeNav.navigate("ConcertDetail", { concertId: item.id })}
            />
          ))}
          {!concertsLoading && !concerts?.length && (
            <EmptyState title="No concerts yet" className="py-8" />
          )}
        </Section>

        <Section
          title="Discover Artists"
          onSeeAll={() => goToSearch("artist")}
          loading={artistsLoading}
        >
          {artists?.map((item) => (
            <ArtistCard
              key={item.id}
              artist={item as ArtistHeader}
              onPress={() => homeNav.navigate("ArtistDetail", { artistId: item.id })}
            />
          ))}
          {!artistsLoading && !artists?.length && (
            <EmptyState title="No artists yet" className="py-8" />
          )}
        </Section>

        <Section
          title="Discover Venues"
          onSeeAll={() => goToSearch("venue")}
          loading={venuesLoading}
        >
          {venues?.map((item) => (
            <VenueCard
              key={item.id}
              venue={item as VenueHeader}
              onPress={() => homeNav.navigate("VenueDetail", { venueId: item.id })}
            />
          ))}
          {!venuesLoading && !venues?.length && (
            <EmptyState title="No venues yet" className="py-8" />
          )}
        </Section>
      </ScrollView>
    </Screen>
  );
}

interface SectionProps {
  title: string;
  onSeeAll: () => void;
  loading: boolean;
  children: React.ReactNode;
}

function Section({ title, onSeeAll, loading, children }: Readonly<SectionProps>) {
  return (
    <View className="mt-5">
      <View className="flex-row items-center justify-between px-4 mb-3">
        <Text className="text-lg font-semibold text-foreground">{title}</Text>
        <Pressable onPress={onSeeAll}>
          <Text className="text-sm font-medium text-primary">See all</Text>
        </Pressable>
      </View>
      {loading ? (
        <View className="flex-row gap-3 px-4">
          {[0, 1, 2].map((i) => (
            <Skeleton key={i} className="w-[180px] h-[200px] rounded-2xl" />
          ))}
        </View>
      ) : (
        <ScrollView
          horizontal
          showsHorizontalScrollIndicator={false}
          contentContainerClassName="px-4 gap-3"
        >
          {children}
        </ScrollView>
      )}
    </View>
  );
}

interface ConcertCardProps {
  concert: ConcertHeader;
  onPress: () => void;
}

function ConcertCard({ concert, onPress }: Readonly<ConcertCardProps>) {
  const { data: src } = useImageUrl(concert.imageUrl);
  return (
    <Pressable onPress={onPress} className="w-44" testID="concert-card">
      <View className="bg-card rounded-2xl border border-border overflow-hidden">
        <Image
          source={{ uri: src }}
          style={{ width: "100%", height: 140 }}
          contentFit="cover"
          className="bg-muted"
        />
        <View className="p-3 gap-1.5">
          <Text className="text-sm font-semibold text-foreground" numberOfLines={1}>{concert.name}</Text>
          <Text className="text-xs text-muted-foreground">
            {dayjs(concert.startDate).format("D MMM YYYY")}
          </Text>
          <View className="flex-row items-center gap-1">
            <MapPin size={11} color={theme.mutedForeground} />
            <Text className="text-xs text-muted-foreground" numberOfLines={1}>
              {concert.town}
            </Text>
          </View>
          {concert.genres?.length > 0 && (
            <GenreChips genres={concert.genres.slice(0, 2)} className="mt-0.5" />
          )}
          {concert.rating != null && <RatingStars rating={concert.rating} />}
        </View>
      </View>
    </Pressable>
  );
}

interface ArtistCardProps {
  artist: ArtistHeader;
  onPress: () => void;
}

function ArtistCard({ artist, onPress }: Readonly<ArtistCardProps>) {
  const { data: src } = useImageUrl(artist.imageUrl);
  return (
    <Pressable onPress={onPress} className="w-36">
      <View className="bg-card rounded-2xl border border-border overflow-hidden items-center p-4 gap-2">
        <Image
          source={{ uri: src }}
          style={{ width: 80, height: 80, borderRadius: 40 }}
          contentFit="cover"
          className="bg-muted rounded-full"
        />
        <Text className="text-sm font-semibold text-foreground text-center" numberOfLines={1}>{artist.name}</Text>
        <View className="flex-row items-center gap-1">
          <MapPin size={11} color={theme.mutedForeground} />
          <Text className="text-xs text-muted-foreground" numberOfLines={1}>{artist.town}</Text>
        </View>
        {artist.rating != null && <RatingStars rating={artist.rating} />}
        {artist.genres?.length > 0 && (
          <GenreChips genres={artist.genres.slice(0, 2)} />
        )}
      </View>
    </Pressable>
  );
}

interface VenueCardProps {
  venue: VenueHeader;
  onPress: () => void;
}

function VenueCard({ venue, onPress }: Readonly<VenueCardProps>) {
  const { data: src } = useImageUrl(venue.imageUrl);
  return (
    <Pressable onPress={onPress} className="w-44">
      <View className="bg-card rounded-2xl border border-border overflow-hidden">
        <Image
          source={{ uri: src }}
          style={{ width: "100%", height: 110 }}
          contentFit="cover"
          className="bg-muted"
        />
        <View className="p-3 gap-1">
          <Text className="text-sm font-semibold text-foreground" numberOfLines={1}>{venue.name}</Text>
          <View className="flex-row items-center gap-1">
            <MapPin size={11} color={theme.mutedForeground} />
            <Text className="text-xs text-muted-foreground" numberOfLines={1}>
              {venue.town}, {venue.county}
            </Text>
          </View>
          {venue.rating != null && <RatingStars rating={venue.rating} />}
        </View>
      </View>
    </Pressable>
  );
}
