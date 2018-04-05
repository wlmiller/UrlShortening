export class ShortUrlRequest {
    url: string;
    customPath: string;

    constructor(url: string, customPath: string) {
        this.url = url;
        this.customPath = customPath;
    }
}
