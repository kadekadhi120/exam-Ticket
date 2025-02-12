CREATE TABLE Category (
    CategoryId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CategoryName NVARCHAR(255) NOT NULL UNIQUE
);

CREATE TABLE Ticket (
    TicketId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- Ubah dari TicketCode jadi GUID
    TicketCode VARCHAR(20) NOT NULL UNIQUE, -- Tetap ada sebagai identifier unik
    TicketName NVARCHAR(255) NOT NULL,
    CategoryId UNIQUEIDENTIFIER NOT NULL,
    TanggalEvent DATE NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Quota INT NOT NULL CHECK (Quota >= 0),
    FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId) ON DELETE CASCADE
);

CREATE TABLE BookedTiket (
    BookedTicketId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TicketId UNIQUEIDENTIFIER NOT NULL, -- Menggunakan TicketId sebagai FK agar lebih aman
    Quantity INT NOT NULL CHECK (Quantity > 0),
    TanggalBooking DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (TicketId) REFERENCES Ticket(TicketId) ON DELETE CASCADE
);
