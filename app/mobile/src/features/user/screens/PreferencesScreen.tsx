import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { ActivityIndicator, ScrollView, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useNavigation } from "@react-navigation/native";
import {
  preferenceRequestSchema,
  useCreateMyPreferenceMutation,
  useMyPreferenceQuery,
  useUpdateMyPreferenceMutation,
  type Preference,
  type PreferenceRequest,
} from "@concertable/customer/features/preferences";
import { useGenresQuery } from "@concertable/shared/features/search";
import type { Genre } from "@concertable/shared/types";
import { GenreChips } from "@concertable/mobile/components/ui/GenreChips";
import { Button } from "@concertable/mobile/components/ui/button";
import { Skeleton } from "@concertable/mobile/components/ui/skeleton";
import { Text } from "@concertable/mobile/components/ui/text";
import { notify } from "@concertable/mobile/lib/toast";
import { theme } from "@concertable/mobile/lib/theme";

const RADIUS_PRESETS = [5, 10, 25, 50, 100] as const;

interface PreferencesFormProps {
  preference?: Preference;
  allGenres?: Genre[];
}

function PreferencesForm({
  preference,
  allGenres,
}: Readonly<PreferencesFormProps>) {
  const nav = useNavigation();
  const updatePreference = useUpdateMyPreferenceMutation();
  const createPreference = useCreateMyPreferenceMutation();
  const {
    handleSubmit,
    setValue,
    watch,
    formState: { isValid },
  } = useForm<PreferenceRequest>({
    resolver: zodResolver(preferenceRequestSchema),
    defaultValues: {
      radiusKm: preference?.radiusKm ?? 25,
      genres: preference?.genres ?? [],
    },
    mode: "onChange",
  });

  const radiusKm = watch("radiusKm");
  const selectedGenres = watch("genres");
  const saving = updatePreference.isPending || createPreference.isPending;

  const onSaved = () => {
    notify("Preferences saved", "success");
    nav.goBack();
  };

  const onValid = (request: PreferenceRequest) => {
    if (preference) {
      updatePreference.mutate(
        { id: preference.id, data: request },
        { onSuccess: onSaved },
      );
      return;
    }

    createPreference.mutate(request, { onSuccess: onSaved });
  };

  const toggleGenre = (genre: Genre) => {
    const genres = selectedGenres.includes(genre)
      ? selectedGenres.filter((selected) => selected !== genre)
      : [...selectedGenres, genre];
    setValue("genres", genres, { shouldDirty: true, shouldValidate: true });
  };

  return (
    <SafeAreaView className="flex-1 bg-background" edges={["bottom"]}>
      <ScrollView
        contentContainerStyle={{ padding: 16, gap: 20 }}
        showsVerticalScrollIndicator={false}
      >
        <View className="gap-3">
          <Text className="text-base font-semibold text-foreground">
            Search Radius
          </Text>
          <View className="flex-row flex-wrap gap-2">
            {RADIUS_PRESETS.map((radius) => (
              <Button
                key={radius}
                variant={radiusKm === radius ? "default" : "outline"}
                size="sm"
                onPress={() =>
                  setValue("radiusKm", radius, {
                    shouldDirty: true,
                    shouldValidate: true,
                  })
                }
              >
                <Text>{`${radius}km`}</Text>
              </Button>
            ))}
          </View>
        </View>

        <View className="gap-3">
          <Text className="text-base font-semibold text-foreground">
            Preferred Genres
          </Text>
          {allGenres && (
            <GenreChips
              genres={allGenres}
              selected={selectedGenres}
              onToggle={toggleGenre}
            />
          )}
        </View>
      </ScrollView>

      <View className="px-4 pt-3 pb-6 border-t border-border">
        <Button
          disabled={saving || !isValid}
          onPress={handleSubmit(onValid)}
          size="lg"
        >
          {saving ? (
            <ActivityIndicator
              size="small"
              color={theme.primaryForeground}
            />
          ) : (
            <Text>Save</Text>
          )}
        </Button>
      </View>
    </SafeAreaView>
  );
}

export function PreferencesScreen() {
  const { data: preference, isLoading } = useMyPreferenceQuery();
  const { data: allGenres } = useGenresQuery();

  if (isLoading) {
    return (
      <View className="flex-1 bg-background p-4 gap-4">
        <Skeleton className="w-full h-12 rounded-xl" />
        <Skeleton className="w-full h-[120px] rounded-xl" />
      </View>
    );
  }

  return <PreferencesForm preference={preference} allGenres={allGenres} />;
}
