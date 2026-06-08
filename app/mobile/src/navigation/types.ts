import type { NavigatorScreenParams } from "@react-navigation/native";
import type { ConcertNavParamList, ProfileStackParamList } from "shared/navigation/types";

export type CustomerConcertNavParamList = ConcertNavParamList & {
  TicketCheckout: { concertId: number };
  CheckoutSuccess: { ticketCount?: number } | undefined;
};

export type HomeStackParamList = {
  HomeMain: undefined;
} & CustomerConcertNavParamList;

export type SearchStackParamList = {
  SearchMain: undefined;
} & CustomerConcertNavParamList;

export type TicketsStackParamList = {
  TicketsMain: undefined;
  TicketDetail: { ticketId: string };
};

export type CustomerProfileStackParamList = ProfileStackParamList & {
  Preferences: undefined;
};

export type CustomerTabParamList = {
  HomeTab: NavigatorScreenParams<HomeStackParamList>;
  SearchTab: NavigatorScreenParams<SearchStackParamList>;
  TicketsTab: NavigatorScreenParams<TicketsStackParamList>;
  Messages: undefined;
  ProfileTab: NavigatorScreenParams<CustomerProfileStackParamList>;
};
