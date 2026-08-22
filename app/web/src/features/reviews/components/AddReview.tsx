import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Star } from "lucide-react";
import type { CreateReviewRequest } from "@concertable/customer/features/reviews";
import { Button } from "@concertable/web/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@concertable/web/components/ui/dialog";
import { Textarea } from "@concertable/web/components/ui/textarea";
import { Label } from "@concertable/web/components/ui/label";
import { useAddReview } from "../hooks/useAddReview";
import {
  createReviewRequestSchema,
  type CreateReviewFormValues,
} from "../schemas/createReviewRequestSchema";

interface Props {
  concertId: number;
}

export function AddReview({ concertId }: Readonly<Props>) {
  const { canReview, isLoading, submit, isPending } = useAddReview(concertId);
  const [open, setOpen] = useState(false);
  const [hovered, setHovered] = useState(0);
  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors, isValid },
  } = useForm<CreateReviewFormValues, unknown, CreateReviewRequest>({
    resolver: zodResolver(createReviewRequestSchema),
    defaultValues: { stars: 0, details: "" },
    mode: "onChange",
  });
  const stars = watch("stars");

  if (isLoading || !canReview) return null;

  function selectStars(value: number) {
    setValue("stars", value, { shouldValidate: true });
  }

  function onValid(request: CreateReviewRequest) {
    submit(request, () => {
      setOpen(false);
      reset();
    });
  }

  return (
    <>
      <Button onClick={() => setOpen(true)}>Add Review</Button>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add Review</DialogTitle>
          </DialogHeader>

          <form onSubmit={handleSubmit(onValid)} className="space-y-4">
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
              {errors.stars && (
                <p className="text-destructive text-xs" data-testid="review-rating-error">
                  {errors.stars.message}
                </p>
              )}
            </div>

            <div className="space-y-1">
              <Label htmlFor="details">Review (optional)</Label>
              <Textarea
                id="details"
                placeholder="Share your experience..."
                rows={4}
                {...register("details")}
              />
            </div>

            <Button
              type="submit"
              disabled={isPending || !isValid}
              className="w-full"
            >
              {isPending ? "Submitting..." : "Submit"}
            </Button>
          </form>
        </DialogContent>
      </Dialog>
    </>
  );
}
