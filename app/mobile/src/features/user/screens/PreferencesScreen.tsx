import { useState } from "react";
import { ActivityIndicator, ScrollView, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useNavigation } from "@react-navigation/native";
import { useMyPreferenceQuery, useUpdateMyPreferenceMutation, useCreateMyPreferenceMutation } from "@customer/shared/features/preferences";
import { useGenresQuery } from "@concertable/shared/features/search";
import type { Genre } from "@concertable/shared/types";
import { GenreChips } from "@/components/ui/GenreChips";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Text } from "@/components/ui/text";
import { notify } from "shared/lib/toast";
import { theme } from "shared/lib/theme";

const RADIUS_PRESETS = [5, 10, 25, 50, 100] as const;

export function PreferencesScreen() {
  const nav = useNavigation();
  const { data: preference, isLoading } = useMyPreferenceQuery();
  const { data: allGenres } = useGenresQuery();
  const updatePreference = useUpdateMyPreferenceMutation();
  const createPreference = useCreateMyPreferenceMutation();

  const [selectedGenres, setSelectedGenres] = useState<Genre[]>(() =>
    preference?.genres ?? [],
  );
  const [radiusKm, setRadiusKm] = useState<number>(() => preference?.radiusKm ?? 25);

  async function handleSave() {
    try {
      if (preference?.id) {
        await updatePreference.mutateAsync({
          id: preference.id,
          data: {
            id: preference.id,
            user: preference.user,
            radiusKm,
            genres: selectedGenres,
          },
        });
      } else {
        await createPreference.mutateAsync({ radiusKm, genres: [] });
      }
      notify("Preferences saved", "success");
      nav.goBack();
    } catch {
      notify("Failed to save preferences", "error");
    }
  }

  const saving = updatePreference.isPending || createPreference.isPending;

  if (isLoading) {
    return (
      <View className="flex-1 bg-background p-4 gap-4">
        <Skeleton className="w-full h-12 rounded-xl" />
        <Skeleton className="w-full h-[120px] rounded-xl" />
      </View>
    );
  }

  return (
    <SafeAreaView className="flex-1 bg-background" edges={["bottom"]}>
      <ScrollView contentContainerStyle={{ padding: 16, gap: 20 }} showsVerticalScrollIndicator={false}>
        <View className="gap-3">
          <Text className="text-base font-semibold text-foreground">Search Radius</Text>
          <View className="flex-row flex-wrap gap-2">
            {RADIUS_PRESETS.map((r) => (
              <Button
                key={r}
                variant={radiusKm === r ? "default" : "outline"}
                size="sm"
                onPress={() => setRadiusKm(r)}
              >
                <Text>{`${r}km`}</Text>
              </Button>
            ))}
          </View>
        </View>

        <View className="gap-3">
          <Text className="text-base font-semibold text-foreground">Preferred Genres</Text>
          {allGenres && (
            <GenreChips
              genres={allGenres}
              selected={selectedGenres}
              onToggle={(genre) =>
                setSelectedGenres((prev) => (prev.includes(genre) ? prev.filter((g) => g !== genre) : [...prev, genre]))
              }
            />
          )}
        </View>
      </ScrollView>

      <View className="px-4 pt-3 pb-6 border-t border-border">
        <Button disabled={saving} onPress={handleSave} size="lg">
          {saving
            ? <ActivityIndicator size="small" color={theme.primaryForeground} />
            : <Text>Save</Text>}
        </Button>
      </View>
    </SafeAreaView>
  );
}
