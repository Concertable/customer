using Concertable.Customer.Ticket.Application.Interfaces;
using Concertable.Customer.Ticket.Infrastructure.Pdf;
using Concertable.Kernel.Exceptions;
using Concertable.Shared.Pdf.Application;
using Moq;
using QuestPDF.Infrastructure;

namespace Concertable.Customer.Ticket.UnitTests.Pdf;

public sealed class TicketPdfServiceTests
{
    [Fact]
    public async Task RenderTicketReceiptAsync_RendersReceiptFromStoredQrCode()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var qrCode = new byte[] { 1, 2, 3 };
        var pdf = new byte[] { 9, 8, 7 };

        var repository = new Mock<ITicketRepository>();
        repository.Setup(r => r.GetQrCodeByIdAsync(ticketId)).ReturnsAsync(qrCode);

        var renderer = new Mock<IPdfRenderer>();
        renderer.Setup(r => r.Render(It.IsAny<IDocument>())).Returns(pdf);

        var sut = new TicketPdfService(renderer.Object, repository.Object);

        // Act
        var result = await sut.RenderTicketReceiptAsync("fan@example.com", ticketId);

        // Assert
        Assert.Same(pdf, result);
        renderer.Verify(r => r.Render(It.Is<IDocument>(d => d is TicketReceiptDocument)), Times.Once);
    }

    [Fact]
    public async Task RenderTicketReceiptAsync_ThrowsNotFound_WhenQrCodeMissing()
    {
        // Arrange
        var ticketId = Guid.NewGuid();

        var repository = new Mock<ITicketRepository>();
        repository.Setup(r => r.GetQrCodeByIdAsync(ticketId)).ReturnsAsync((byte[]?)null);

        var renderer = new Mock<IPdfRenderer>();

        var sut = new TicketPdfService(renderer.Object, repository.Object);

        // Act / Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.RenderTicketReceiptAsync("fan@example.com", ticketId));
        renderer.Verify(r => r.Render(It.IsAny<IDocument>()), Times.Never);
    }
}
