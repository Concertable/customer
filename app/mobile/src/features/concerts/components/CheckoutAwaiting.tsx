import { ActivityIndicator, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { Check } from "lucide-react-native";
import { Text } from "@/components/ui/text";
import { theme } from "shared/lib/theme";

type StepStatus = "done" | "active" | "pending";

interface Step {
  label: string;
  status: StepStatus;
}

const STEPS_AWAITING: Step[] = [
  { label: "Payment authorised", status: "done" },
  { label: "Confirming with our system", status: "active" },
  { label: "Issuing your tickets", status: "pending" },
];

const STEPS_TIMEOUT: Step[] = [
  { label: "Payment authorised", status: "done" },
  { label: "Still confirming — we'll notify you", status: "active" },
  { label: "Issuing your tickets", status: "pending" },
];

interface Props {
  timed_out?: boolean;
}

export function CheckoutAwaiting({ timed_out = false }: Readonly<Props>) {
  const steps = timed_out ? STEPS_TIMEOUT : STEPS_AWAITING;

  return (
    <SafeAreaView className="flex-1 bg-background" edges={["bottom"]}>
      <View className="flex-1 items-center justify-center gap-8 px-6">
        <ActivityIndicator size="large" color={theme.primary} />

        <View className="items-center gap-2">
          <Text className="text-xl font-bold text-foreground text-center">
            {timed_out ? "Still confirming your payment" : "Processing your payment"}
          </Text>
          {timed_out && (
            <Text className="text-sm text-muted-foreground text-center leading-relaxed">
              Your tickets will appear in your profile once confirmed.
            </Text>
          )}
        </View>

        <View className="w-full gap-3">
          {steps.map((step) => (
            <StepRow key={step.label} step={step} />
          ))}
        </View>
      </View>
    </SafeAreaView>
  );
}

function StepRow({ step }: { step: Step }) {
  return (
    <View className="flex-row items-center gap-3">
      <View
        className="w-6 h-6 rounded-full items-center justify-center"
        style={{
          backgroundColor:
            step.status === "done"
              ? theme.success
              : step.status === "active"
                ? theme.primary
                : theme.muted,
        }}
      >
        {step.status === "done" ? (
          <Check size={13} color="#fff" strokeWidth={2.5} />
        ) : step.status === "active" ? (
          <ActivityIndicator size="small" color="#fff" />
        ) : null}
      </View>
      <Text
        className={`text-sm ${
          step.status === "pending" ? "text-muted-foreground" : "text-foreground"
        } font-medium`}
      >
        {step.label}
      </Text>
    </View>
  );
}
