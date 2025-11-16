# Tren Rezervasyon API Projesi

Bu proje, Ada Yazılım şirketi için .NET 9 Minimal API kullanılarak geliştirilmiş bir teknik değerlendirme projesidir.

Bu API, bir trendeki vagonların doluluk oranlarına (proje dokümanında %70 olarak belirtilmiştir) ve rezervasyon yaptırılacak müşteri sayısına göre rezervasyon yapılmasının mümkün olup olmadığını hesaplamaktadır.

Çalışan API URL'i aşağıda bulunmaktadır.

**LINK:** []

## Projede Kullanılan Teknolojiler
- NET 9.0
- C# 13.0
- Swagger (Deployment Öncesi Test İçin Kullanıldı)
- Postman (Deployment Sonrası Test İçin Kullanıldı)

## Projeyi Yerel Bilgisayarda Nasıl Çalıştırabilirsiniz
1. Bu repository'i, 'git clone [URL]' komutu ile klonlayın.
2. Proje klasörüne gidin ve 'dotnet restore' komutu ile gerekli paketleri yükleyin.
3. 'dotnet run' komutunu kullanarak projeyi çalıştırın.
4. Test arayüzü (Swagger) için 'http://localhost:5032/swagger/index.html' linkini tarayıcınızda açınız.
5. Açılan ekranda POST tuşuna ve ardından "Try it out" butonlarına tıklayınız. Aşağıda örnek bir API isteği bulunmaktadır.

## Örnek API İsteği (JSON Formatında)
```json
{
  "Train": {
    "Name": "Başkent Ekspres",
    "Wagons": [
      {"Name": "Vagon 1", "Capacity": 100, "FullSeats": 68},
      {"Name": "Vagon 2", "Capacity": 90, "FullSeats": 50},
      {"Name": "Vagon 3", "Capacity": 80, "FullSeats": 80}
    ]
  },
  "ReservationCount": 3,
  "DifferentVagonsAllowed": true
}
```

## Başarılı Yanıt Örneği
```json
{
  "reservationAvailable": true,
  "placementDetails": [
    { "wagonName": "Vagon 1", "reservedCustomerCount": 2 },
    { "wagonName": "Vagon 2", "reservedCustomerCount": 1 }
  ]
}
```
