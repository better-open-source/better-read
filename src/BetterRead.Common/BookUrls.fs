module BetterRead.Common.BookUrls

let baseUrl = "http://loveread.ec"

let bookUrl bookId =
    let path = sprintf "/view_global.php?id=%d" bookId
    baseUrl + path

let bookCover bookId =
    let path = sprintf "/img/photo_books/%d.jpg" bookId
    baseUrl + path

let bookPage bookId pageId =
    let path = sprintf "/read_book.php?id=%d&p=%d" bookId pageId
    baseUrl + path

let bookContentImage bookId imageId =
    let path = sprintf "/img/photo_books/%d/i_%3d.jpg" bookId imageId
    baseUrl + path