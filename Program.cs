using System;
using System.Data;
using Npgsql;

class KutuphaneYonetim
{
    private const string ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password= (şifre) ;Database= (veritabanı adı)";

    public static void Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("\nKütüphane Yönetim Sistemi");
            Console.WriteLine("1. Çalışan İşlem Menüsü");
            Console.WriteLine("2. Üye İşlem Menüsü"); // Yeni seçenek eklendi
            Console.WriteLine("3. Kitap İşlem Menüsü");
            Console.WriteLine("4. Bağış İşlem Menüsü"); // Yeni seçenek eklendi
            Console.WriteLine("5. Etkinlik İşlem Menüsü ");
            Console.WriteLine("6. Gider Hesaplama ");
            Console.Write("Seçiminizi yapın: ");

            int secim = int.Parse(Console.ReadLine());

            switch (secim)
            {
                case 1:
                    CalisanIslemMenusu();
                    break;
                case 2:
                    UyeIslemMenusu();
                    break;
                case 3:
                    KitapIslemMenusu();
                    break;
                case 4:
                    BagisIslemMenusu();
                    break;
                case 5:
                    EtkinlikIslemMenusu(); // Etkinlik menüsünü çağır
                    break;
                case 6:
                    MaliyetHesaplaVeEkle();
                    break;
                default:
                    Console.WriteLine("Geçersiz seçim!");
                    break;
            }
        }
    }
    private static void CalisanIslemMenusu()
    {
        Console.WriteLine(" \nÇalışan İşlem Menüsü");
        Console.WriteLine("1. Çalışan Ekle");
        Console.WriteLine("2. Çalışan Sil");
        Console.WriteLine("3. Çalışan Güncelle"); // Yeni seçenek eklendi
        Console.Write("Seçiminizi yapın: ");

        int secim = int.Parse(Console.ReadLine());

        switch (secim)
        {
            case 1:
                CalisanEkle();
                break;
            case 2:
                CalisanSil();
                break;
            case 3:
                CalisanGuncelle();
                break;
            default:
                Console.WriteLine("Geçersiz seçim!");
                break;
        }
    }
    private static void CalisanEkle()
    {
        Console.Clear();
        Console.WriteLine(" \n* Çalışan Ekle *");
        // Kullanıcıdan bilgiler alınır
        Console.Write("Ad: ");
        string ad = Console.ReadLine();

        Console.Write("Soyad: ");
        string soyad = Console.ReadLine();

        Console.Write("Telefon No: ");
        string telefonNo = Console.ReadLine();

        Console.Write("E-posta: ");
        string mail = Console.ReadLine();

        Console.Write("Adres: ");
        string adres = Console.ReadLine();

        try
        {
            // Veritabanı bağlantısını aç
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                // Fonksiyonu çağır
                using (var cmd = new NpgsqlCommand("SELECT calisan_ekle(@ad, @soyad, @telefonNo, @mail, @adres)", connection))
                {
                    // Parametreleri ekleyin
                    cmd.Parameters.AddWithValue("ad", ad);
                    cmd.Parameters.AddWithValue("soyad", soyad);
                    cmd.Parameters.AddWithValue("telefonNo", telefonNo);
                    cmd.Parameters.AddWithValue("mail", mail);
                    cmd.Parameters.AddWithValue("adres", adres);

                    // Fonksiyonu çalıştır
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("\nÇalışan başarıyla eklendi.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Hata: " + ex.Message);
        }
    }
    public static void CalisanSil()
    {
        Console.Write("\nSilmek istediğiniz çalışanın kişi ID'sini girin: ");
        if (int.TryParse(Console.ReadLine(), out int kisiId))
        {
            using (NpgsqlConnection baglanti = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    baglanti.Open();

                    // Çalışan var mı kontrol et
                    string kontrolQuery = "SELECT COUNT(*) FROM Calisanlar WHERE kisiID = @kisiID";
                    using (var kontrolCmd = new NpgsqlCommand(kontrolQuery, baglanti))
                    {
                        kontrolCmd.Parameters.AddWithValue("@kisiID", kisiId);
                        int count = (int)(long)kontrolCmd.ExecuteScalar();
                        if (count == 0)
                        {
                            Console.WriteLine("Böyle bir çalışan yok.");
                            return;
                        }
                    }

                    using (NpgsqlCommand komut = new NpgsqlCommand("CALL calisan_sil(@p_kisi_id);", baglanti))
                    {
                        komut.Parameters.AddWithValue("p_kisi_id", kisiId);
                        komut.ExecuteNonQuery();

                        Console.WriteLine($"\nKişi ID {kisiId} ile ilişkilendirilen çalışan başarıyla silindi.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Hata: " + ex.Message);
                }
            }
        }
        else
        {
            Console.WriteLine("Geçersiz bir ID girdiniz. Lütfen bir sayı girin.");
        }
    }
    private static void CalisanGuncelle()
    {
        using (var conn = new NpgsqlConnection(ConnectionString))
        {
            conn.Open();
            string query = "SELECT c.kisiID, k.ad, k.soyad FROM Calisanlar c JOIN Kisiler k ON c.kisiID = k.kisiID";

            using (var cmd = new NpgsqlCommand(query, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("Çalışanlar:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"ID: {reader["kisiID"]}, Ad: {reader["ad"]}, Soyad: {reader["soyad"]}");
                    }
                }
            }

            Console.Write("\nGüncellemek istediğiniz çalışanın ID'sini girin: ");
            int kisiID = int.Parse(Console.ReadLine());

            Console.Write("Yeni Ad: ");
            string yeniAd = Console.ReadLine();

            Console.Write("Yeni Soyad: ");
            string yeniSoyad = Console.ReadLine();

            Console.Write("Yeni Telefon No: ");
            string yeniTelefonNo = Console.ReadLine();

            Console.Write("Yeni E-posta: ");
            string yeniMail = Console.ReadLine();

            Console.Write("Yeni Adres: ");
            string yeniAdres = Console.ReadLine();

            string updateQuery = @"
                UPDATE Kisiler
                SET ad = @yeniAd, soyad = @yeniSoyad, telefonNo = @yeniTelefonNo, mail = @yeniMail, adres = @yeniAdres
                WHERE kisiID = @kisiID";

            using (var updateCmd = new NpgsqlCommand(updateQuery, conn))
            {
                updateCmd.Parameters.AddWithValue("@yeniAd", yeniAd);
                updateCmd.Parameters.AddWithValue("@yeniSoyad", yeniSoyad);
                updateCmd.Parameters.AddWithValue("@yeniTelefonNo", yeniTelefonNo);
                updateCmd.Parameters.AddWithValue("@yeniMail", yeniMail);
                updateCmd.Parameters.AddWithValue("@yeniAdres", yeniAdres);
                updateCmd.Parameters.AddWithValue("@kisiID", kisiID);

                int affectedRows = updateCmd.ExecuteNonQuery();
                if (affectedRows > 0)
                {
                    Console.WriteLine("\nÇalışan bilgileri başarıyla güncellendi.");
                }
                else
                {
                    Console.WriteLine("\nÇalışan bilgileri güncellenemedi.");
                }
            }
        }
    }
    private static void UyeIslemMenusu()
    {
        Console.WriteLine("\nÜye İşlem Menüsü");
        Console.WriteLine("1. Üye Ekle");
        Console.WriteLine("2. Üye Sil");
        Console.WriteLine("3. Üye Güncelle"); // Yeni seçenek eklendi
        Console.Write("Seçiminizi yapın: ");

        int secim = int.Parse(Console.ReadLine());

        switch (secim)
        {
            case 1:
                UyeEkle();
                break;
            case 2:
                UyeSil();
                break;
            case 3:
                UyeGuncelle();
                break;
            default:
                Console.WriteLine("Geçersiz seçim!");
                break;
        }
    }
    private static void UyeEkle()
    {
        Console.WriteLine("\nYeni Üye Ekle");
        Console.Write("Ad: ");
        string ad = Console.ReadLine();

        Console.Write("Soyad: ");
        string soyad = Console.ReadLine();

        Console.Write("Telefon Numarası: ");
        string telefonNo = Console.ReadLine();

        Console.Write("Mail Adresi: ");
        string mail = Console.ReadLine();

        Console.Write("Adres: ");
        string adres = Console.ReadLine();

        using (NpgsqlConnection baglanti = new NpgsqlConnection(ConnectionString))
        {
            try
            {
                baglanti.Open();

                using (NpgsqlCommand komut = new NpgsqlCommand("SELECT uye_ekle(@ad, @soyad, @telefonNo, @mail, @adres);", baglanti))
                {
                    komut.Parameters.AddWithValue("ad", ad);
                    komut.Parameters.AddWithValue("soyad", soyad);
                    komut.Parameters.AddWithValue("telefonNo", telefonNo);
                    komut.Parameters.AddWithValue("mail", mail);
                    komut.Parameters.AddWithValue("adres", adres);

                    komut.ExecuteNonQuery();

                    Console.WriteLine("\nÜye başarıyla eklendi.");
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                Console.WriteLine($"Veritabanı hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }

    }
    public static void UyeSil()
    {
        Console.WriteLine("\nÜye Sil");
        Console.Write("\nSilmek istediğiniz üyenin ID'sini girin: ");

        if (!int.TryParse(Console.ReadLine(), out int kisiID))
        {
            Console.WriteLine("Geçerli bir ID giriniz!");
            return;
        }

        using (NpgsqlConnection baglanti = new NpgsqlConnection(ConnectionString))
        {
            try
            {
                baglanti.Open();

                // Üye var mı kontrol et
                string kontrolQuery = "SELECT COUNT(*) FROM Uyeler WHERE kisiID = @kisiID";
                using (var kontrolCmd = new NpgsqlCommand(kontrolQuery, baglanti))
                {
                    kontrolCmd.Parameters.AddWithValue("@kisiID", kisiID);
                    int count = (int)(long)kontrolCmd.ExecuteScalar();
                    if (count == 0)
                    {
                        Console.WriteLine("Böyle bir üye yok.");
                        return;
                    }
                }

                using (NpgsqlCommand komut = new NpgsqlCommand("CALL uye_sil(@kisiID);", baglanti))
                {
                    komut.Parameters.AddWithValue("kisiID", kisiID);

                    komut.ExecuteNonQuery();

                    Console.WriteLine("\nÜye başarıyla silindi.");
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                Console.WriteLine($"Veritabanı hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }

    }
    private static void UyeGuncelle()
    {
        using (var conn = new NpgsqlConnection(ConnectionString))
        {
            conn.Open();
            string query = "SELECT u.kisiID, k.ad, k.soyad FROM Uyeler u JOIN Kisiler k ON u.kisiID = k.kisiID";

            using (var cmd = new NpgsqlCommand(query, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("\nÜyeler:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"ID: {reader["kisiID"]}, Ad: {reader["ad"]}, Soyad: {reader["soyad"]}");
                    }
                }
            }

            Console.Write("\nGüncellemek istediğiniz üyenin ID'sini girin: ");
            int kisiID = int.Parse(Console.ReadLine());

            Console.Write("Yeni Ad: ");
            string yeniAd = Console.ReadLine();

            Console.Write("Yeni Soyad: ");
            string yeniSoyad = Console.ReadLine();

            Console.Write("Yeni Telefon No: ");
            string yeniTelefonNo = Console.ReadLine();

            Console.Write("Yeni E-posta: ");
            string yeniMail = Console.ReadLine();

            Console.Write("Yeni Adres: ");
            string yeniAdres = Console.ReadLine();

            string updateQuery = @"
                UPDATE Kisiler
                SET ad = @yeniAd, soyad = @yeniSoyad, telefonNo = @yeniTelefonNo, mail = @yeniMail, adres = @yeniAdres
                WHERE kisiID = @kisiID";

            using (var updateCmd = new NpgsqlCommand(updateQuery, conn))
            {
                updateCmd.Parameters.AddWithValue("@yeniAd", yeniAd);
                updateCmd.Parameters.AddWithValue("@yeniSoyad", yeniSoyad);
                updateCmd.Parameters.AddWithValue("@yeniTelefonNo", yeniTelefonNo);
                updateCmd.Parameters.AddWithValue("@yeniMail", yeniMail);
                updateCmd.Parameters.AddWithValue("@yeniAdres", yeniAdres);
                updateCmd.Parameters.AddWithValue("@kisiID", kisiID);

                int affectedRows = updateCmd.ExecuteNonQuery();
                if (affectedRows > 0)
                {
                    Console.WriteLine("\nÜye bilgileri başarıyla güncellendi.");
                }
                else
                {
                    Console.WriteLine("\nÜye bilgileri güncellenemedi.");
                }
            }
        }
    }
    private static void KitapIslemMenusu()
    {
        Console.WriteLine("\nKitap İşlem Menüsü");
        Console.WriteLine("1. Ödünç Verme");
        Console.WriteLine("2. İade Alma");
        Console.WriteLine("3. Kitapları Listele");
        Console.WriteLine("4. Kitap Ara");
        Console.WriteLine("5. Kitap Tedarik");
        Console.WriteLine("6. Kategoriye Göre Kitap Listele"); // Yeni seçenek eklendi
        Console.Write("Seçiminizi yapın: ");

        int secim = int.Parse(Console.ReadLine());

        switch (secim)
        {
            case 1:
                OduncVer();
                break;
            case 2:
                IadeAl();
                break;
            case 3:
                KitaplariListele();
                break;
            case 4:
                KitapAra();
                break;
            case 5:
                KitapAlimEkle();
                break;
            case 6:
                KategoriyeGoreKitapListele();
                break;
            default:
                Console.WriteLine("Geçersiz seçim!");
                break;
        }
    }
    private static void KategoriyeGoreKitapListele()
    {
        using (var conn = new NpgsqlConnection(ConnectionString))
        {
            conn.Open();
            string kategoriQuery = "SELECT kategoriKodu, ad FROM Kitap_Kategorileri";

            using (var kategoriCmd = new NpgsqlCommand(kategoriQuery, conn))
            {
                using (var reader = kategoriCmd.ExecuteReader())
                {
                    Console.WriteLine("\nKategoriler:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"Kategori Kodu: {reader["kategoriKodu"]}, Adı: {reader["ad"]}");
                    }
                }
            }

            Console.Write("\nListelenmesini istediğiniz kategorinin kodunu girin: ");
            int kategoriKodu = int.Parse(Console.ReadLine());

            string kitapQuery = @"
                SELECT 
                    k.KitapID, 
                    k.ad AS KitapAdi, 
                    CONCAT(ki.ad, ' ', ki.soyad) AS YazarAdi, 
                    ya.ad AS YayineviAdi, 
                    k.yayinYili, 
                    CASE 
                        WHEN k.durum = TRUE THEN 'Mevcut' 
                        ELSE 'Ödünçte' 
                    END AS Durum
                FROM 
                    Kitaplar k
                JOIN 
                    Yazarlar y ON k.yazarID = y.kisiID
                JOIN 
                    Kisiler ki ON y.kisiID = ki.kisiID
                JOIN 
                    Yayinevi ya ON k.yayinEviID = ya.yayineviID
                WHERE 
                    k.kategoriID = @kategoriKodu";

            using (var kitapCmd = new NpgsqlCommand(kitapQuery, conn))
            {
                kitapCmd.Parameters.AddWithValue("@kategoriKodu", kategoriKodu);
                using (var reader = kitapCmd.ExecuteReader())
                {
                    Console.WriteLine("Kitaplar:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"Kitap ID: {reader["KitapID"]}, Adı: {reader["KitapAdi"]}, Yazar: {reader["YazarAdi"]}, Yayinevi: {reader["YayineviAdi"]}, Yayın Yılı: {reader["yayinYili"]}, Durum: {reader["Durum"]}");
                    }
                }
            }
        }
    }
    private static void KitaplariListele()
    {
        using (var conn = new NpgsqlConnection(ConnectionString))
        {
            conn.Open();
            string query = @"
                SELECT 
                    k.KitapID, 
                    k.ad AS KitapAdi, 
                    CONCAT(ki.ad, ' ', ki.soyad) AS YazarAdi, 
                    ya.ad AS YayineviAdi, 
                    kk.ad AS KategoriAdi, 
                    k.yayinYili, 
                    CASE 
                        WHEN k.durum = TRUE THEN 'Mevcut' 
                        ELSE 'Ödünçte' 
                    END AS Durum
                FROM 
                    Kitaplar k
                JOIN 
                    Yazarlar y ON k.yazarID = y.kisiID
                JOIN 
                    Kisiler ki ON y.kisiID = ki.kisiID
                JOIN 
                    Yayinevi ya ON k.yayinEviID = ya.yayineviID
                JOIN 
                    Kitap_Kategorileri kk ON k.kategoriID = kk.kategoriKodu
                ORDER BY 
                    k.KitapID ASC"; // Kitapları ID'ye göre artan sırada listele

            using (var cmd = new NpgsqlCommand(query, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("\nKitaplar:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"Kitap ID: {reader["KitapID"]}, Adı: {reader["KitapAdi"]}, Yazar: {reader["YazarAdi"]}, Yayinevi: {reader["YayineviAdi"]}, Kategori: {reader["KategoriAdi"]}, Yayın Yılı: {reader["yayinYili"]}, Durum: {reader["Durum"]}");
                    }
                }
            }
        }
    }
    private static void KitapAra()
    {
        Console.WriteLine("\nKitap Arama Menüsü");
        Console.WriteLine("1. Kitap ID'ye Göre Ara");
        Console.WriteLine("2. Kitap Adına Göre Ara");
        Console.Write("Seçiminizi yapın: ");
        int secim = int.Parse(Console.ReadLine());

        switch (secim)
        {
            case 1:
                KitapIDyeGoreAra();
                break;
            case 2:
                KitapAdinaGoreAra();
                break;
            default:
                Console.WriteLine("Geçersiz seçim!");
                break;
        }
    }

    private static void KitapIDyeGoreAra()
    {
        Console.Write("\nKitap ID: ");
        int kitapID = int.Parse(Console.ReadLine());

        using (var conn = new NpgsqlConnection(ConnectionString))
        {
            conn.Open();
            string query = @"
                SELECT 
                    k.KitapID, 
                    k.ad AS KitapAdi, 
                    COALESCE(l.lokasyonID, k.lokasyonID) AS lokasyonID, 
                    COALESCE(s.ad, 'Bilinmiyor') AS SubeAdi, 
                    COALESCE(s.adres, 'Bilinmiyor') AS SubeAdresi
                FROM 
                    Kitaplar k
                LEFT JOIN 
                    Lokasyonlar l ON k.KitapID = l.kitapID
                LEFT JOIN 
                    Subeler s ON l.SubeID = s.SubeID
                WHERE 
                    k.KitapID = @kitapID";

            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@kitapID", kitapID);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Console.WriteLine($"Kitap ID: {reader["KitapID"]}, Adı: {reader["KitapAdi"]}, Lokasyon ID: {reader["lokasyonID"]}, Şube: {reader["SubeAdi"]}, Adres: {reader["SubeAdresi"]}");
                    }
                    else
                    {
                        Console.WriteLine("Kitap bulunamadı.");
                    }
                }
            }
        }
    }

   
    private static void KitapAdinaGoreAra()
    {
        Console.Write("\nKitap Adı: ");
        string kitapAdi = Console.ReadLine();

        using (var conn = new NpgsqlConnection(ConnectionString))
        {
            conn.Open();
            string query = @"
                SELECT 
                    k.KitapID, 
                    k.ad AS KitapAdi, 
                    COALESCE(l.lokasyonID, k.lokasyonID) AS lokasyonID, 
                    COALESCE(s.ad, 'Bilinmiyor') AS SubeAdi, 
                    COALESCE(s.adres, 'Bilinmiyor') AS SubeAdresi
                FROM 
                    Kitaplar k
                LEFT JOIN 
                    Lokasyonlar l ON k.KitapID = l.kitapID
                LEFT JOIN 
                    Subeler s ON l.SubeID = s.SubeID
                WHERE 
                    k.ad ILIKE @kitapAdi";

            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@kitapAdi", "%" + kitapAdi + "%");
                using (var reader = cmd.ExecuteReader())
                {
                    bool kitapBulundu = false;
                    while (reader.Read())
                    {
                        kitapBulundu = true;
                        Console.WriteLine($"Kitap ID: {reader["KitapID"]}, Adı: {reader["KitapAdi"]}, Lokasyon ID: {reader["lokasyonID"]}, Şube: {reader["SubeAdi"]}, Adres: {reader["SubeAdresi"]}");
                    }
                    if (!kitapBulundu)
                    {
                        Console.WriteLine("Kitap bulunamadı.");
                    }
                }
            }
        }
    }
    private static void OduncVer()
    {
        Console.Write("\nKitap ID: ");
        int kitapID = int.Parse(Console.ReadLine());
        Console.Write("Üye ID: ");
        int uyeID = int.Parse(Console.ReadLine());

        using (var conn = new NpgsqlConnection(ConnectionString))
        {
            conn.Open();

            // Kitap var mı kontrol et
            string kitapKontrolQuery = "SELECT COUNT(*) FROM Kitaplar WHERE KitapID = @kitapID";
            using (var kitapKontrolCmd = new NpgsqlCommand(kitapKontrolQuery, conn))
            {
                kitapKontrolCmd.Parameters.AddWithValue("@kitapID", kitapID);
                int kitapCount = (int)(long)kitapKontrolCmd.ExecuteScalar();
                if (kitapCount == 0)
                {
                    Console.WriteLine("\nKitap bulunamadı. Lütfen geçerli bir kitap ID girin.");
                    return;
                }
            }

            // Üye var mı kontrol et
            string uyeKontrolQuery = "SELECT COUNT(*) FROM Uyeler WHERE kisiID = @uyeID";
            using (var uyeKontrolCmd = new NpgsqlCommand(uyeKontrolQuery, conn))
            {
                uyeKontrolCmd.Parameters.AddWithValue("@uyeID", uyeID);
                int uyeCount = (int)(long)uyeKontrolCmd.ExecuteScalar();
                if (uyeCount == 0)
                {
                    Console.WriteLine("\nÜye bulunamadı. Lütfen geçerli bir üye ID girin.");
                    return;
                }
            }

            // Kitap ödünçte mi kontrol et
            string oduncteMiQuery = "SELECT durum FROM Kitaplar WHERE KitapID = @kitapID";
            using (var oduncteMiCmd = new NpgsqlCommand(oduncteMiQuery, conn))
            {
                oduncteMiCmd.Parameters.AddWithValue("@kitapID", kitapID);
                bool oduncteMi = (bool)oduncteMiCmd.ExecuteScalar();
                if (!oduncteMi)
                {
                    Console.WriteLine("\nKitap zaten ödünçte. Lütfen geçerli bir kitap ID girin.");
                    return;
                }
            }

            using (var cmd = new NpgsqlCommand("SELECT kitap_odunc_verme(@kitapID, @uyeID)", conn))
            {
                cmd.Parameters.AddWithValue("@kitapID", kitapID);
                cmd.Parameters.AddWithValue("@uyeID", uyeID);
                cmd.ExecuteNonQuery();
                Console.WriteLine("\nKitap ödünç verildi.");
            }
        }
    }
    private static void IadeAl()
    {
        Console.Write("\nKitap ID: ");
        int kitapID = int.Parse(Console.ReadLine());
        Console.Write("Üye ID: ");
        int uyeID = int.Parse(Console.ReadLine());

        using (var conn = new NpgsqlConnection(ConnectionString))
        {
            conn.Open();

            // Kitap var mı kontrol et
            string kitapKontrolQuery = "SELECT COUNT(*) FROM Kitaplar WHERE KitapID = @kitapID";
            using (var kitapKontrolCmd = new NpgsqlCommand(kitapKontrolQuery, conn))
            {
                kitapKontrolCmd.Parameters.AddWithValue("@kitapID", kitapID);
                int kitapCount = (int)(long)kitapKontrolCmd.ExecuteScalar();
                if (kitapCount == 0)
                {
                    Console.WriteLine("\nKitap bulunamadı. Lütfen geçerli bir kitap ID girin.");
                    return;
                }
            }

            // Üyenin bu kitabı ödünç alıp almadığını kontrol et
            string oduncKontrolQuery = @"
            SELECT COUNT(*) 
            FROM Islemler 
            WHERE kitapID = @kitapID AND uyeID = @uyeID AND islemTuru = 'ödünç'";
            using (var oduncKontrolCmd = new NpgsqlCommand(oduncKontrolQuery, conn))
            {
                oduncKontrolCmd.Parameters.AddWithValue("@kitapID", kitapID);
                oduncKontrolCmd.Parameters.AddWithValue("@uyeID", uyeID);
                int oduncCount = (int)(long)oduncKontrolCmd.ExecuteScalar();
                if (oduncCount == 0)
                {
                    Console.WriteLine("\nBu kitap bu üye tarafından ödünç alınmamış.");
                    return;
                }
            }

            // İade işlemini kaydet
            string iadeEkleQuery = @"
            INSERT INTO Islemler (kitapID, uyeID, islemTuru) 
            VALUES (@kitapID, @uyeID, 'iade')";
            using (var iadeEkleCmd = new NpgsqlCommand(iadeEkleQuery, conn))
            {
                iadeEkleCmd.Parameters.AddWithValue("@kitapID", kitapID);
                iadeEkleCmd.Parameters.AddWithValue("@uyeID", uyeID);
                iadeEkleCmd.ExecuteNonQuery();
            }

            // Kitabın durumunu true yap
            string kitapDurumGuncelleQuery = "UPDATE Kitaplar SET durum = TRUE WHERE KitapID = @kitapID";
            using (var kitapDurumGuncelleCmd = new NpgsqlCommand(kitapDurumGuncelleQuery, conn))
            {
                kitapDurumGuncelleCmd.Parameters.AddWithValue("@kitapID", kitapID);
                kitapDurumGuncelleCmd.ExecuteNonQuery();
                Console.WriteLine("Kitap başarıyla iade alındı ve durumu güncellendi.");
            }
        }
    }
    public static void KitapAlimEkle()
    {
        Console.WriteLine("\nKitap Alım Ekle");

        Console.Write("Tedarikçi ID: ");
        int tedarikciID = int.Parse(Console.ReadLine());

        Console.Write("Kitap ID: ");
        int kitapID = int.Parse(Console.ReadLine());

        Console.Write("Şube Kodu: ");
        int subeKodu = int.Parse(Console.ReadLine());

        Console.Write("Miktar: ");
        int miktar = int.Parse(Console.ReadLine());

        Console.Write("Fiyat: ");
        decimal fiyat = decimal.Parse(Console.ReadLine());

        // Bugünün tarihini al
        DateTime tarih = DateTime.Now;

        using (NpgsqlConnection baglanti = new NpgsqlConnection(ConnectionString))
        {
            try
            {
                baglanti.Open();

                // Kitap alımını ekleyen komut
                using (NpgsqlCommand komut = new NpgsqlCommand(
                    "INSERT INTO Kitap_Alim (tedarikciID, tarih, miktar, fiyat, kitapID, subeKodu) VALUES (@tedarikciID, @tarih, @miktar, @fiyat, @kitapID, @subeKodu);",
                    baglanti))
                {
                    komut.Parameters.AddWithValue("tedarikciID", tedarikciID);
                    komut.Parameters.AddWithValue("tarih", tarih);  // Bugünün tarihini ekliyoruz
                    komut.Parameters.AddWithValue("miktar", miktar);
                    komut.Parameters.AddWithValue("fiyat", fiyat);
                    komut.Parameters.AddWithValue("kitapID", kitapID);
                    komut.Parameters.AddWithValue("subeKodu", subeKodu);

                    komut.ExecuteNonQuery();

                    Console.WriteLine("\nKitap alımı başarıyla eklendi.");
                }

                // Kitapları ekle
                for (int i = 0; i < miktar; i++)
                {
                    // Kitap bilgilerini al
                    string selectQuery = "SELECT * FROM Kitaplar WHERE KitapID = @kitapID";
                    using (var selectCmd = new NpgsqlCommand(selectQuery, baglanti))
                    {
                        selectCmd.Parameters.AddWithValue("@kitapID", kitapID);
                        using (var reader = selectCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string kitapAdi = reader["ad"].ToString();
                                int yazarID = (int)reader["yazarID"];
                                int yayinEviID = (int)reader["yayinEviID"];
                                int kategoriID = (int)reader["kategoriID"];
                                int yayinYili = (int)reader["yayinYili"];

                                reader.Close();

                                // Yeni kitap ekle
                                using (var kitapCmd = new NpgsqlCommand("INSERT INTO Kitaplar (ad, yazarID, yayinEviID, kategoriID, yayinYili, subeKodu) VALUES (@ad, @yazarID, @yayinEviID, @kategoriID, @yayinYili, @subeKodu)", baglanti))
                                {
                                    kitapCmd.Parameters.AddWithValue("@ad", kitapAdi);
                                    kitapCmd.Parameters.AddWithValue("@yazarID", yazarID);
                                    kitapCmd.Parameters.AddWithValue("@yayinEviID", yayinEviID);
                                    kitapCmd.Parameters.AddWithValue("@kategoriID", kategoriID);
                                    kitapCmd.Parameters.AddWithValue("@yayinYili", yayinYili);
                                    kitapCmd.Parameters.AddWithValue("@subeKodu", subeKodu);
                                    kitapCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                Console.WriteLine($"Veritabanı hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }
    }
    private static void BagisIslemMenusu()
    {
        Console.WriteLine("\nBağış İşlem Menüsü");
        Console.WriteLine("1. Bağış Ekle");
        Console.WriteLine("2. Bağışları Listele");
        Console.Write("Seçiminizi yapın: ");

        int secim = int.Parse(Console.ReadLine());

        switch (secim)
        {
            case 1:
                BagisEkle();
                break;
            case 2:
                BagislariListele();
                break;
            default:
                Console.WriteLine("Geçersiz seçim!");
                break;
        }
    }
    private static void BagisEkle()
    {
        Console.Write("\nÜye ID: ");
        int uyeID = int.Parse(Console.ReadLine());
        Console.Write("Kitap Adeti: ");
        int kitapAdeti = int.Parse(Console.ReadLine());
        Console.Write("Açıklama: ");
        string aciklama = Console.ReadLine();

        using (var conn = new NpgsqlConnection(ConnectionString))
        {
            conn.Open();
            using (var cmd = new NpgsqlCommand("SELECT bagis_ekle(@uyeID, @kitapAdeti, @aciklama)", conn))
            {
                cmd.Parameters.AddWithValue("@uyeID", uyeID);
                cmd.Parameters.AddWithValue("@kitapAdeti", kitapAdeti);
                cmd.Parameters.AddWithValue("@aciklama", aciklama);
                cmd.ExecuteNonQuery();
                Console.WriteLine("\nBağış başarıyla eklendi.");
            }

            // Kitapları ekle
            for (int i = 0; i < kitapAdeti; i++)
            {
                // Rastgele bir kitap seç
                string selectQuery = "SELECT * FROM Kitaplar ORDER BY RANDOM() LIMIT 1";
                using (var selectCmd = new NpgsqlCommand(selectQuery, conn))
                {
                    using (var reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string kitapAdi = reader["ad"].ToString();
                            int yazarID = (int)reader["yazarID"];
                            int yayinEviID = (int)reader["yayinEviID"];
                            int kategoriID = (int)reader["kategoriID"];
                            int yayinYili = (int)reader["yayinYili"];
                            int subeKodu = (int)reader["subeKodu"];

                            reader.Close();

                            // Yeni kitap ekle
                            using (var kitapCmd = new NpgsqlCommand("INSERT INTO Kitaplar (ad, yazarID, yayinEviID, kategoriID, yayinYili, subeKodu) VALUES (@ad, @yazarID, @yayinEviID, @kategoriID, @yayinYili, @subeKodu)", conn))
                            {
                                kitapCmd.Parameters.AddWithValue("@ad", kitapAdi);
                                kitapCmd.Parameters.AddWithValue("@yazarID", yazarID);
                                kitapCmd.Parameters.AddWithValue("@yayinEviID", yayinEviID);
                                kitapCmd.Parameters.AddWithValue("@kategoriID", kategoriID);
                                kitapCmd.Parameters.AddWithValue("@yayinYili", yayinYili);
                                kitapCmd.Parameters.AddWithValue("@subeKodu", subeKodu);
                                kitapCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }
    }
    private static void BagislariListele()
    {
        using (var conn = new NpgsqlConnection(ConnectionString))
        {
            conn.Open();
            using (var cmd = new NpgsqlCommand("SELECT * FROM Bagis", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("\nBağışlar:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"Bağış ID: {reader["bagisID"]}, Üye ID: {reader["uyeID"]}, Kitap Adeti: {reader["kitapAdeti"]}, Tarih: {reader["tarih"]}, Açıklama: {reader["aciklama"]}");
                    }
                }
            }
        }
    }
    public static void MaliyetHesaplaVeEkle()
    {
        try
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();

                // Kitap alımlarının fiyatlarını toplayan sorgu
                string query = @"
                    SELECT SUM(fiyat * miktar)
                    FROM Kitap_Alim
                    WHERE tarih >= CURRENT_DATE - INTERVAL '30 day'";  // Son 30 gün

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    decimal toplamMaliyet = 0;

                    // Fiyatları topla
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        toplamMaliyet = Convert.ToDecimal(result);
                    }

                    // Maliyet tablosuna yeni maliyeti ekle
                    string insertQuery = @"
                        INSERT INTO Maliyet (toplamMaliyet, alimID, hesaplanmaTarihi)
                        VALUES (@toplamMaliyet, @alimID, CURRENT_DATE);";

                    using (var insertCmd = new NpgsqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("toplamMaliyet", toplamMaliyet);
                        insertCmd.Parameters.AddWithValue("alimID", 1);  // Burada ilgili alimID'yi kullanabilirsiniz.

                        insertCmd.ExecuteNonQuery();
                        Console.WriteLine($"\nToplam gider: {toplamMaliyet} TL.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata oluştu: {ex.Message}");
        }
    }
    private static void EtkinlikIslemMenusu()
    {
        Console.WriteLine("\nEtkinlik İşlem Menüsü");
        Console.WriteLine("1. Etkinlik Ekle");
        Console.WriteLine("2. Etkinlik Listele");
        Console.Write("Seçiminizi yapın: ");

        int secim = int.Parse(Console.ReadLine());

        switch (secim)
        {
            case 1:
                EtkinlikEkle();
                break;
            case 2:
                EtkinlikListele();
                break;
            default:
                Console.WriteLine("Geçersiz seçim!");
                break;
        }
    }
    private static void EtkinlikEkle()
    {
        // Kullanıcıdan etkinlik bilgilerini al
        Console.Write("\nÇalışan ID: ");
        int calisanID = int.Parse(Console.ReadLine());

        Console.Write("Etkinlik Adı: ");
        string ad = Console.ReadLine();

        Console.Write("Tarih (yyyy-mm-dd): ");
        DateTime tarih = DateTime.Parse(Console.ReadLine());

        Console.Write("Açıklama: ");
        string aciklama = Console.ReadLine();

        try
        {
            // Veritabanına bağlan
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();

                // Etkinlik ekleme fonksiyonunu çağır
                using (var cmd = new NpgsqlCommand("SELECT etkinlik_ekle(@calisanID, @ad, @tarih, @aciklama)", conn))
                {
                    cmd.Parameters.AddWithValue("@calisanID", calisanID);
                    cmd.Parameters.AddWithValue("@ad", ad);
                    cmd.Parameters.AddWithValue("@tarih", tarih);
                    cmd.Parameters.AddWithValue("@aciklama", aciklama);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("\nEtkinlik başarıyla eklendi.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Hata: " + ex.Message);
        }
    }
    private static void EtkinlikListele()
    {
        try
        {
            // Veritabanına bağlan
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();

                // Etkinlik listeleme fonksiyonunu çağır
                using (var cmd = new NpgsqlCommand("SELECT * FROM etkinlik_listele();", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    // Etkinlikleri yazdır
                    while (reader.Read())
                    {
                        Console.WriteLine($"\nEtkinlik ID: {reader["etkinlikID"]}");
                        Console.WriteLine($"Çalışan ID: {reader["calisanID"]}");
                        Console.WriteLine($"Ad: {reader["ad"]}");
                        Console.WriteLine($"Tarih: {reader["tarih"]}");
                        Console.WriteLine($"Açıklama: {reader["aciklama"]}");
                        Console.WriteLine("-----------------------------");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Hata: " + ex.Message);
        }
    }

}
