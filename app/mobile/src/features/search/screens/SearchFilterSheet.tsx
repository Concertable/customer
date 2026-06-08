import { forwardRef, useCallback, useMemo, useRef, useState } from "react";
import { ActivityIndicator, Pressable, ScrollView, View } from "react-native";
import { BottomSheetModal, BottomSheetScrollView } from "@gorhom/bottom-sheet";
import { Calendar } from "react-native-calendars";
import { MapPin, X } from "lucide-react-native";
import { useCurrentLocation } from "shared/hooks/useCurrentLocation";
import { useSearchFiltersStore, useGenresQuery } from "@concertable/shared/features/search";
import type { HeaderType, SearchFilters } from "@concertable/shared/features/search";
import type { Genre } from "@concertable/shared/types";
import { GenreChips } from "@/components/ui/GenreChips";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { Text } from "@/components/ui/text";
import { theme } from "shared/lib/theme";
import { HEADER_TYPE_OPTIONS } from "shared/features/search/constants";
import dayjs from "dayjs";

const RADIUS_PRESETS = [5, 10, 25, 50, 100] as const;
const SNAP_POINTS = ["90%"];

const SORT_OPTIONS = [
  { value: "date" as const, label: "Date" },
  { value: "name" as const, label: "Name" },
  { value: "distance" as const, label: "Distance" },
];

type SortValue = "date" | "name" | "distance";

interface Props {}

export const SearchFilterSheet = forwardRef<BottomSheetModal, Props>(function SearchFilterSheet(_, ref) {
  const internalRef = useRef<BottomSheetModal>(null);

  const mergedRef = useCallback(
    (node: BottomSheetModal | null) => {
      (internalRef as React.MutableRefObject<BottomSheetModal | null>).current = node;
      if (typeof ref === "function") ref(node);
      else if (ref) (ref as React.MutableRefObject<BottomSheetModal | null>).current = node;
    },
    [ref],
  );

  const { filters, setFilters } = useSearchFiltersStore();
  const { data: allGenres } = useGenresQuery();

  const [headerType, setHeaderType] = useState<HeaderType>("concert");
  const [genres, setGenres] = useState<Genre[]>([]);
  const [from, setFrom] = useState<string | undefined>();
  const [to, setTo] = useState<string | undefined>();
  const [lat, setLat] = useState<number | undefined>();
  const [lng, setLng] = useState<number | undefined>();
  const [locationLabel, setLocationLabel] = useState<string | undefined>();
  const [radius, setRadius] = useState(25);
  const [sortOrder, setSortOrder] = useState<SortValue>("date");
  const { requestLocation, locating } = useCurrentLocation();

  const filtersRef = useRef(filters);
  filtersRef.current = filters;

  const snapPoints = useMemo(() => SNAP_POINTS, []);

  const handleChange = useCallback((index: number) => {
    if (index >= 0) {
      const f = filtersRef.current;
      setHeaderType(f.headerType ?? "concert");
      setGenres(f.genres ?? []);
      setFrom(f.from);
      setTo(f.to);
      setLat(f.lat);
      setLng(f.lng);
      setLocationLabel(f.locationLabel);
      setRadius(f.radius ?? 25);
      setSortOrder((f.orderBy as SortValue | undefined) ?? "date");
    }
  }, []);

  const markedDates = useMemo(() => {
    if (!from) return {};
    if (!to)
      return { [from]: { selected: true, color: theme.primary, textColor: theme.primaryForeground } };
    const result: Record<string, object> = {};
    let current = dayjs(from);
    const end = dayjs(to);
    while (!current.isAfter(end)) {
      const d = current.format("YYYY-MM-DD");
      result[d] = {
        startingDay: d === from,
        endingDay: d === to,
        color: theme.primary,
        textColor: theme.primaryForeground,
      };
      current = current.add(1, "day");
    }
    return result;
  }, [from, to]);

  function handleDayPress({ dateString }: { dateString: string }) {
    if (!from || (from && to)) {
      setFrom(dateString);
      setTo(undefined);
    } else if (dayjs(dateString).isBefore(dayjs(from))) {
      setTo(from);
      setFrom(dateString);
    } else {
      setTo(dateString);
    }
  }

  async function handleUseLocation() {
    const result = await requestLocation();
    if (result) {
      setLat(result.lat);
      setLng(result.lng);
      setLocationLabel(result.label);
    }
  }

  function handleApply() {
    setFilters({
      ...filtersRef.current,
      headerType,
      genres: genres.length ? genres : undefined,
      from,
      to,
      lat,
      lng,
      locationLabel: lat != null ? locationLabel : undefined,
      radius: lat != null ? radius : undefined,
      orderBy: sortOrder !== "date" ? sortOrder : undefined,
    });
    internalRef.current?.dismiss();
  }

  function handleReset() {
    setHeaderType("concert");
    setGenres([]);
    setFrom(undefined);
    setTo(undefined);
    setLat(undefined);
    setLng(undefined);
    setLocationLabel(undefined);
    setRadius(25);
    setSortOrder("date");
    setFilters({ headerType: "concert" });
    internalRef.current?.dismiss();
  }

  function toggleGenre(genre: Genre) {
    setGenres((prev) => (prev.includes(genre) ? prev.filter((g) => g !== genre) : [...prev, genre]));
  }

  return (
    <BottomSheetModal
      ref={mergedRef}
      snapPoints={snapPoints}
      onChange={handleChange}
      enablePanDownToClose
      backgroundStyle={{ backgroundColor: theme.background }}
      handleIndicatorStyle={{ backgroundColor: theme.border }}
    >
      <View className="flex-row items-center justify-between px-4 pb-3 border-b border-border">
        <Text className="text-lg font-semibold text-foreground">Filters</Text>
        <Pressable onPress={() => internalRef.current?.dismiss()}>
          <X size={20} color={theme.mutedForeground} />
        </Pressable>
      </View>

      <BottomSheetScrollView contentContainerStyle={{ paddingBottom: 120 }}>
        <Section title="Type">
          <Tabs value={headerType} onValueChange={(v) => setHeaderType(v as HeaderType)}>
            <TabsList className="w-full">
              {HEADER_TYPE_OPTIONS.map((opt) => (
                <TabsTrigger key={opt.value} value={opt.value} className="flex-1">
                  <Text>{opt.label}</Text>
                </TabsTrigger>
              ))}
            </TabsList>
          </Tabs>
        </Section>

        <Section title="Genres">
          {allGenres && allGenres.length > 0 ? (
            <GenreChips genres={allGenres} selected={genres} onToggle={toggleGenre} />
          ) : (
            <Text className="text-sm text-muted-foreground">Loading genres…</Text>
          )}
        </Section>

        <Section title="Date Range">
          {(from || to) && (
            <View className="flex-row items-center justify-between mb-3">
              <Text className="text-sm text-foreground">
                {from ? dayjs(from).format("D MMM YYYY") : "—"}
                {" – "}
                {to ? dayjs(to).format("D MMM YYYY") : "—"}
              </Text>
              <Pressable onPress={() => { setFrom(undefined); setTo(undefined); }}>
                <Text className="text-xs text-primary font-medium">Clear</Text>
              </Pressable>
            </View>
          )}
          <Calendar
            markingType="period"
            markedDates={markedDates}
            onDayPress={handleDayPress}
            minDate={dayjs().format("YYYY-MM-DD")}
            theme={{
              selectedDayBackgroundColor: theme.primary,
              todayTextColor: theme.primary,
              arrowColor: theme.primary,
              dotColor: theme.primary,
              backgroundColor: theme.background,
              calendarBackground: theme.background,
              textSectionTitleColor: theme.mutedForeground,
              dayTextColor: theme.foreground,
              monthTextColor: theme.foreground,
            }}
          />
        </Section>

        <Section title="Location">
          {lat != null ? (
            <View className="gap-3">
              <View className="flex-row items-center justify-between">
                <View className="flex-row items-center gap-2 flex-1">
                  <MapPin size={14} color={theme.primary} />
                  <Text className="text-sm text-foreground" numberOfLines={1}>
                    {locationLabel ?? `${lat.toFixed(3)}, ${lng?.toFixed(3)}`}
                  </Text>
                </View>
                <Pressable onPress={() => { setLat(undefined); setLng(undefined); setLocationLabel(undefined); }}>
                  <Text className="text-xs text-primary font-medium">Clear</Text>
                </Pressable>
              </View>
              <Text className="text-xs text-muted-foreground">Radius</Text>
              <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={{ gap: 8 }}>
                {RADIUS_PRESETS.map((r) => (
                  <Pressable
                    key={r}
                    onPress={() => setRadius(r)}
                    className={`px-4 py-2 rounded-full border ${radius === r ? "bg-primary border-primary" : "border-border bg-background"}`}
                  >
                    <Text className={`text-sm font-medium ${radius === r ? "text-primary-foreground" : "text-foreground"}`}>
                      {r}km
                    </Text>
                  </Pressable>
                ))}
              </ScrollView>
            </View>
          ) : (
            <Pressable
              onPress={handleUseLocation}
              disabled={locating}
              className="flex-row items-center gap-2 self-start border border-border rounded-2xl px-4 py-2.5"
            >
              {locating ? (
                <ActivityIndicator size="small" color={theme.primary} />
              ) : (
                <MapPin size={14} color={theme.primary} />
              )}
              <Text className="text-sm font-medium text-primary">
                {locating ? "Getting location…" : "Use my location"}
              </Text>
            </Pressable>
          )}
        </Section>

        <Section title="Sort By">
          <Tabs value={sortOrder} onValueChange={(v) => setSortOrder(v as SortValue)}>
            <TabsList className="w-full">
              {SORT_OPTIONS.map((opt) => (
                <TabsTrigger key={opt.value} value={opt.value} className="flex-1">
                  <Text>{opt.label}</Text>
                </TabsTrigger>
              ))}
            </TabsList>
          </Tabs>
        </Section>
      </BottomSheetScrollView>

      <View className="absolute bottom-0 left-0 right-0 flex-row gap-3 px-4 pt-3 pb-8 border-t border-border bg-background">
        <Button variant="outline" onPress={handleReset} className="flex-1"><Text>Reset</Text></Button>
        <Button onPress={handleApply} className="flex-1"><Text>Apply</Text></Button>
      </View>
    </BottomSheetModal>
  );
});

interface SectionProps {
  title: string;
  children: React.ReactNode;
}

function Section({ title, children }: Readonly<SectionProps>) {
  return (
    <View className="px-4 pt-5 pb-2">
      <Text className="text-base font-semibold text-foreground mb-3">{title}</Text>
      {children}
    </View>
  );
}
