import { useRef } from "react";
import { useMountEffect } from "@concertable/shared/hooks/useMountEffect";
import { Pressable, ScrollView, Share, Text, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useRoute } from "@react-navigation/native";
import type { RouteProp } from "@react-navigation/native";
import QRCode from "react-native-qrcode-svg";
import * as Brightness from "expo-brightness";
import { CalendarDays, Hash, Mail, MapPin, Music, Share2, Ticket } from "lucide-react-native";
import type { LucideIcon } from "lucide-react-native";
import { useUpcomingTicketsQuery, useTicketHistoryQuery } from "@customer/shared/features/tickets";
import { ErrorState } from "@/components/ui/ErrorState";
import { theme } from "shared/lib/theme";
import dayjs from "dayjs";
import type { TicketsStackParamList } from "../../../navigation/types";

type TicketDetailRoute = RouteProp<TicketsStackParamList, "TicketDetail">;

export function TicketDetailScreen() {
  const route = useRoute<TicketDetailRoute>();
  const { ticketId } = route.params;

  const { data: upcoming } = useUpcomingTicketsQuery();
  const { data: history } = useTicketHistoryQuery();

  const ticket = [...(upcoming ?? []), ...(history ?? [])].find((t) => t.id === ticketId);

  const prevBrightness = useRef<number | null>(null);

  useMountEffect(() => {
    Brightness.getBrightnessAsync().then((b) => {
      prevBrightness.current = b;
      Brightness.setBrightnessAsync(1);
    });
    return () => {
      if (prevBrightness.current !== null) {
        Brightness.setBrightnessAsync(prevBrightness.current);
      }
    };
  });

  if (!ticket) {
    return (
      <SafeAreaView className="flex-1 bg-background" edges={["bottom"]}>
        <ErrorState message="Ticket not found." />
      </SafeAreaView>
    );
  }

  async function handleShare() {
    await Share.share({ message: `I'm going to ${ticket!.concert.name}! 🎵` });
  }

  return (
    <SafeAreaView className="flex-1 bg-background" edges={["bottom"]}>
      <View className="flex-row justify-end px-4 py-2">
        <Pressable onPress={handleShare} className="p-2">
          <Share2 size={20} color={theme.primary} />
        </Pressable>
      </View>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={{ padding: 16, gap: 16 }}>
        <View className="bg-card rounded-3xl border-2 border-primary/20 p-6 items-center gap-4">
          <Text className="text-lg font-bold text-foreground text-center" numberOfLines={2}>
            {ticket.concert.name}
          </Text>
          <Text className="text-sm text-muted-foreground">
            {dayjs(ticket.concert.startDate).format("ddd D MMM YYYY")}
          </Text>
          <View className="bg-white p-4 rounded-2xl" testID="ticket-qr">
            <QRCode value={ticket.qrCode} size={240} />
          </View>
          <Text className="text-xs text-muted-foreground font-mono" numberOfLines={1}>
            {ticket.id}
          </Text>
        </View>

        <View className="bg-card rounded-2xl border border-border overflow-hidden">
          <DetailRow icon={Music} label={ticket.concert.artistName} />
          <DetailRow icon={MapPin} label={ticket.concert.venueName} />
          <DetailRow icon={CalendarDays} label={dayjs(ticket.concert.startDate).format("ddd D MMM YYYY")} />
          <DetailRow icon={Ticket} label={`£${ticket.concert.price.toFixed(2)}`} />
          <DetailRow icon={Mail} label={ticket.userEmail} />
          <DetailRow icon={Hash} label={ticket.id} last />
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

function DetailRow({ icon: Icon, label, last }: { icon: LucideIcon; label: string; last?: boolean }) {
  return (
    <View className={`flex-row items-center gap-3 px-4 py-3.5 ${!last ? "border-b border-border" : ""}`}>
      <Icon size={16} color={theme.mutedForeground} />
      <Text className="text-sm text-foreground flex-1" numberOfLines={1}>{label}</Text>
    </View>
  );
}
