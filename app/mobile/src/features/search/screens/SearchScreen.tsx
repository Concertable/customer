import { useRef, useState } from "react";
import { Keyboard, View } from "react-native";
import { useNavigation } from "@react-navigation/native";
import type { NativeStackNavigationProp } from "@react-navigation/native-stack";
import type { BottomSheetModal } from "@gorhom/bottom-sheet";
import { useAutocompleteQuery, useSearchFilters } from "@concertable/shared/features/search";
import type { AutocompleteResult } from "@concertable/shared/features/search";
import { Screen } from "@concertable/mobile/components/ui/Screen";
import { Navbar } from "@concertable/mobile/components/ui/Navbar";
import { theme } from "@concertable/mobile/lib/theme";
import type { SearchStackParamList } from "../../../navigation/types";
import { SearchBar } from "@concertable/mobile/features/search/components/SearchBar";
import { FilterChipsRow } from "@concertable/mobile/features/search/components/FilterChipsRow";
import { AutocompleteList } from "@concertable/mobile/features/search/components/AutocompleteList";
import { SearchResults } from "@concertable/mobile/features/search/components/SearchResults";
import { SearchFilterSheet } from "./SearchFilterSheet";

type SearchNav = NativeStackNavigationProp<SearchStackParamList>;

export function SearchScreen() {
  const nav = useNavigation<SearchNav>();
  const { filters } = useSearchFilters();
  const [focused, setFocused] = useState(false);
  const filterSheetRef = useRef<BottomSheetModal>(null);

  const { data: autocomplete } = useAutocompleteQuery(filters.query ?? "", filters.headerType);
  const showAutocomplete = focused && !!filters.query && !!autocomplete?.length;

  function handleAutocompleteSelect(item: AutocompleteResult) {
    Keyboard.dismiss();
    if (item.$type === "concert") nav.navigate("ConcertDetail", { concertId: item.id });
    else if (item.$type === "artist") nav.navigate("ArtistDetail", { artistId: item.id });
    else nav.navigate("VenueDetail", { venueId: item.id });
  }

  return (
    <Screen padded={false} header={<Navbar />}>
      <View style={{ borderBottomWidth: 1, borderColor: theme.border }}>
        <SearchBar
          focused={focused}
          onFocusChange={setFocused}
          onOpenFilters={() => filterSheetRef.current?.present()}
        />
        <FilterChipsRow />
      </View>

      {showAutocomplete ? (
        <AutocompleteList
          query={filters.query!}
          headerType={filters.headerType}
          onSelect={handleAutocompleteSelect}
        />
      ) : (
        <SearchResults />
      )}

      <SearchFilterSheet ref={filterSheetRef} />
    </Screen>
  );
}
