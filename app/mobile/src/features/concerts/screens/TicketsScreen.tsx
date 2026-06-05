import { useRef, useState } from "react";
import { FlatList, Pressable, RefreshControl, View } from "react-native";
import { useNavigation } from "@react-navigation/native";
import type { NativeStackNavigationProp } from "@react-navigation/native-stack";
import QRCode from "react-native-qrcode-svg";
import { CalendarDays, MapPin, Music, Ticket as TicketIcon } from "lucide-react-native";
import { useUpcomingTicketsQuery, useTicketHistoryQuery } from "@customer/shared/features/tickets";
import type { Ticket } from "@customer/shared/features/tickets";
import { Screen } from "@/components/ui/Screen";
import { Navbar } from "@/components/ui/Navbar";
import { EmptyState } from "@/components/ui/EmptyState";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Skeleton } from "@/components/ui/skeleton";
import { Text } from "@/components/ui/text";
import { theme } from "shared/lib/theme";
import dayjs from "dayjs";
import type { TicketsStackParamList } from "../../../navigation/types";

const TAB_OPTIONS = [
  { value: "upcoming" as const, label: "Upcoming" },
  { value: "history" as const, label: "History" },
];

type TicketsNav = NativeStackNavigationProp<TicketsStackParamList>;

export function TicketsScreen() {
  const nav = useNavigation<TicketsNav>();
  const [tab, setTab] = useState<"upcoming" | "history">("upcoming");

  const { data: upcoming, isLoading: upLoading, refetch: refetchUpcoming } = useUpcomingTicketsQuery();
  const { data: history, isLoading: histLoading, refetch: refetchHistory } = useTicketHistoryQuery();

  const tickets = tab === "upcoming" ? (upcoming ?? []) : (history ?? []);
  const isLoading = tab === "upcoming" ? upLoading : histLoading;
  const refreshing = upLoading || histLoading;

  function onRefresh() {
    refetchUpcoming();
    refetchHistory();
  }

  return (
    <Screen padded={false} header={<Navbar />}>
      <View className="px-4 pt-3 pb-3 border-b border-border">
        <Tabs value={tab} onValueChange={(v) => setTab(v as typeof tab)}>
          <TabsList className="w-full">
            {TAB_OPTIONS.map((opt) => (
              <TabsTrigger key={opt.value} value={opt.value} className="flex-1">
                <Text>{opt.label}</Text>
              </TabsTrigger>
            ))}
          </TabsList>
        </Tabs>
      </View>

      {isLoading ? (
        <View className="p-4 gap-3">
          {[0, 1, 2, 3].map((i) => (
            <Skeleton key={i} className="w-full h-[90px] rounded-2xl" />
          ))}
        </View>
      ) : (
        <FlatList
          data={tickets}
          keyExtractor={(item) => item.id}
          showsVerticalScrollIndicator={false}
          contentContainerStyle={{ padding: 16, gap: 12 }}
          refreshControl={
            <RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor={theme.primary} />
          }
          renderItem={({ item }) => (
            <TicketCard
              ticket={item}
              onPress={() => nav.navigate("TicketDetail", { ticketId: item.id })}
            />
          )}
          ListEmptyComponent={
            <EmptyState
              icon={TicketIcon}
              title={tab === "upcoming" ? "No upcoming tickets" : "No past tickets"}
              description="Tickets you purchase will appear here"
              className="mt-8"
            />
          }
        />
      )}
    </Screen>
  );
}

function TicketCard({ ticket, onPress }: { ticket: Ticket; onPress: () => void }) {
  return (
    <Pressable onPress={onPress} testID="ticket-card">
      <View className="bg-card rounded-2xl border border-border flex-row items-center p-3 gap-3">
        <View className="w-16 h-16 rounded-xl bg-primary/15 items-center justify-center shrink-0">
          <Text className="text-xl font-bold text-primary">
            {ticket.concert.name.charAt(0).toUpperCase()}
          </Text>
        </View>

        <View className="flex-1 gap-1 min-w-0">
          <Text className="text-sm font-semibold text-foreground" numberOfLines={1}>
            {ticket.concert.name}
          </Text>
          <View className="flex-row items-center gap-1">
            <Music size={11} color={theme.mutedForeground} />
            <Text className="text-xs text-muted-foreground" numberOfLines={1}>
              {ticket.concert.artistName}
            </Text>
          </View>
          <View className="flex-row items-center gap-1">
            <MapPin size={11} color={theme.mutedForeground} />
            <Text className="text-xs text-muted-foreground" numberOfLines={1}>
              {ticket.concert.venueName}
            </Text>
          </View>
          <View className="flex-row items-center gap-1">
            <CalendarDays size={11} color={theme.mutedForeground} />
            <Text className="text-xs text-muted-foreground">
              {dayjs(ticket.concert.startDate).format("D MMM YYYY")}
            </Text>
          </View>
        </View>

        <View className="shrink-0 rounded-lg overflow-hidden bg-white p-1">
          <QRCode value={ticket.qrCode} size={44} />
        </View>
      </View>
    </Pressable>
  );
}
