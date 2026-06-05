import {
  Dialog,
  DialogContent,
  DialogTrigger,
  DialogTitle,
} from "@/components/ui/dialog";

interface Props {
  qrCode: string;
  alt: string;
}

export function QrPopover({ qrCode, alt }: Readonly<Props>) {
  const src = `data:image/png;base64,${qrCode}`;

  return (
    <Dialog>
      <DialogTrigger
        data-testid="qr-trigger"
        className="cursor-zoom-in self-center rounded border bg-white p-1 transition-shadow hover:shadow-md focus:outline-none focus-visible:ring-2"
        aria-label={alt}
      >
        <img src={src} alt={alt} className="size-32" />
      </DialogTrigger>
      <DialogContent data-testid="qr-dialog" className="w-auto max-w-[90vw] bg-white p-6">
        <DialogTitle className="sr-only">{alt}</DialogTitle>
        <img src={src} alt={alt} data-testid="qr-image" className="size-[min(80vw,480px)]" />
      </DialogContent>
    </Dialog>
  );
}
