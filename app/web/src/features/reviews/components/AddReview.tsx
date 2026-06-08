import { useState } from "react";
import { toast } from "sonner";
import { Star } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { useAddReview } from "../hooks/useAddReview";

interface Props {
  concertId: number;
}

export function AddReview({ concertId }: Readonly<Props>) {
  const { canReview, isLoading, mutation } = useAddReview(concertId);
  const [open, setOpen] = useState(false);
  const [stars, setStars] = useState(0);
  const [hovered, setHovered] = useState(0);
  const [details, setDetails] = useState("");

  if (isLoading || !canReview) return null;

  async function handleSubmit() {
    if (stars === 0) {
      toast.error("Please select a star rating");
      return;
    }
    try {
      await mutation.mutateAsync({ stars, details: details || undefined });
      toast.success("Review submitted");
      setOpen(false);
      setStars(0);
      setDetails("");
    } catch {
      toast.error("Failed to submit review");
    }
  }

  return (
    <>
      <Button onClick={() => setOpen(true)}>Add Review</Button>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add Review</DialogTitle>
          </DialogHeader>

          <div className="space-y-4">
            <div className="space-y-1">
              <Label>Rating</Label>
              <div className="flex gap-1">
                {Array.from({ length: 5 }).map((_, i) => (
                  <button
                    key={i}
                    type="button"
                    onMouseEnter={() => setHovered(i + 1)}
                    onMouseLeave={() => setHovered(0)}
                    onClick={() => setStars(i + 1)}
                  >
                    <Star
                      className={`size-6 transition-colors ${
                        i < (hovered || stars)
                          ? "fill-gold text-gold"
                          : "text-muted-foreground"
                      }`}
                    />
                  </button>
                ))}
              </div>
            </div>

            <div className="space-y-1">
              <Label htmlFor="details">Review (optional)</Label>
              <Textarea
                id="details"
                placeholder="Share your experience..."
                value={details}
                onChange={(e) => setDetails(e.target.value)}
                rows={4}
              />
            </div>

            <Button
              onClick={() => void handleSubmit()}
              disabled={mutation.isPending}
              className="w-full"
            >
              {mutation.isPending ? "Submitting..." : "Submit"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}
