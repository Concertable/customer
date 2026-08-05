import { useState } from "react";
import { Star } from "lucide-react";
import { Button } from "@concertable/web/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@concertable/web/shared/components/ui/dialog";
import { Textarea } from "@concertable/web/shared/components/ui/textarea";
import { Label } from "@concertable/web/shared/components/ui/label";
import { useAddReview } from "../hooks/useAddReview";

interface Props {
  concertId: number;
}

export function AddReview({ concertId }: Readonly<Props>) {
  const { canReview, isLoading, submit, isPending } = useAddReview(concertId);
  const [open, setOpen] = useState(false);
  const [stars, setStars] = useState(0);
  const [hovered, setHovered] = useState(0);
  const [details, setDetails] = useState("");
  const [error, setError] = useState<string | null>(null);

  if (isLoading || !canReview) return null;

  function selectStars(value: number) {
    setStars(value);
    setError(null);
  }

  function handleSubmit() {
    const parsed = submit({ stars, details }, () => {
      setOpen(false);
      setStars(0);
      setDetails("");
      setError(null);
    });
    if (!parsed.success) setError(parsed.error.issues[0].message);
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
                    onClick={() => selectStars(i + 1)}
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
              {error && (
                <p className="text-destructive text-xs" data-testid="review-rating-error">
                  {error}
                </p>
              )}
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
              onClick={handleSubmit}
              disabled={isPending}
              className="w-full"
            >
              {isPending ? "Submitting..." : "Submit"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}
