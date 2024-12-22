-- Kisiler Tablosu
CREATE TABLE Kisiler (
    kisiID SERIAL PRIMARY KEY,
    ad VARCHAR(255) NOT NULL,
    soyad VARCHAR(255) NOT NULL,
    telefonNo VARCHAR(11),
    mail VARCHAR(255),
    adres TEXT
);

-- Yazarlar Tablosu
CREATE TABLE Yazarlar (
    kisiID INT PRIMARY KEY REFERENCES Kisiler(kisiID)
);

-- Çalışanlar Tablosu
CREATE TABLE Calisanlar (
    kisiID INT PRIMARY KEY REFERENCES Kisiler(kisiID),
    baslamaTarihi DATE
);

-- Üyeler Tablosu
CREATE TABLE Uyeler (
    kisiID INT PRIMARY KEY REFERENCES Kisiler(kisiID),
    uyelikTarihi DATE
);

-- Yayinevi Tablosu
CREATE TABLE Yayinevi (
    yayineviID SERIAL PRIMARY KEY,
    ad VARCHAR(255) NOT NULL,
    iletisimBilgisi TEXT
);

-- Kitap Kategorileri Tablosu
CREATE TABLE Kitap_Kategorileri (
    kategoriKodu SERIAL PRIMARY KEY,
    ad VARCHAR(255) NOT NULL
);

-- Kitaplar Tablosu
CREATE TABLE Subeler (
    SubeID SERIAL PRIMARY KEY,
    ad  VARCHAR(255) NOT NULL,
    adres TEXT,
    telefon VARCHAR(11)
   
);

CREATE TABLE Lokasyonlar (
    lokasyonID INT PRIMARY KEY, -- Otomatik artan ID
    SubeID INT REFERENCES Subeler(SubeID),
    kitapID INT UNIQUE -- Her kitap için bir lokasyon
);
-- Kitaplar Tablosu
CREATE TABLE Kitaplar (
    KitapID SERIAL PRIMARY KEY,
    ad VARCHAR(255) NOT NULL,
    yazarID INT REFERENCES Yazarlar(kisiID),
    yayinEviID INT REFERENCES Yayinevi(yayineviID),
    kategoriID INT REFERENCES Kitap_Kategorileri(kategoriKodu),
    yayinYili INT,
    durum BOOLEAN DEFAULT TRUE,
    lokasyonID INT DEFAULT floor(random() * 1000000),  -- 0-999999 arası rastgele değer
    subeKodu INT NOT NULL -- Şube kodu dışarıdan girilecek
);

-- Kitap adını index olarak tanımla
CREATE INDEX idx_kitap_ad ON Kitaplar(ad);

-- Etkinlikler Tablosu
CREATE TABLE Etkinlikler (
    etkinlikID SERIAL PRIMARY KEY,
    calisanID INT REFERENCES Calisanlar(kisiID),
    ad VARCHAR(255),
    tarih TIMESTAMP,
    aciklama TEXT
);

-- Islemler Tablosu
CREATE TABLE Islemler (
    islemID SERIAL PRIMARY KEY,
    kitapID INT REFERENCES Kitaplar(KitapID),
    uyeID INT REFERENCES Uyeler(kisiID),
    islemTuru VARCHAR(20) CHECK (islemTuru IN ('ödünç', 'iade'))
);

-- Bagis Tablosu
CREATE TABLE Bagis (
    bagisID SERIAL PRIMARY KEY,
    uyeID INT REFERENCES Uyeler(kisiID),
    kitapAdeti INT,
    tarih DATE,
    aciklama TEXT
);

-- Tedarikçi Tablosu
CREATE TABLE Tedarikci (
    tedarikciID SERIAL PRIMARY KEY,
    ad VARCHAR(255) NOT NULL,
    iletisim TEXT
);

-- Kitap Alim Tablosu
CREATE TABLE Kitap_Alim (
    alimID SERIAL PRIMARY KEY,
    tedarikciID INT REFERENCES Tedarikci(tedarikciID),
    tarih DATE,
    miktar INT,
    fiyat NUMERIC(10, 2),
    kitapID INT REFERENCES Kitaplar(KitapID),
    subeKodu INT NOT NULL
);

CREATE TABLE Maliyet (
    maliyetID SERIAL PRIMARY KEY,
    alimID INT REFERENCES Kitap_Alim(alimID),  -- Kitap Alım tablosuyla ilişki
    toplamMaliyet NUMERIC(10, 2),               -- Hesaplanan toplam maliyet
    hesaplanmaTarihi DATE DEFAULT CURRENT_DATE  -- Maliyet hesaplama tarihi
);

-- Trigger fonksiyonu
CREATE OR REPLACE FUNCTION kitap_lokasyon_random()
RETURNS TRIGGER AS $$
DECLARE
    rastgeleLokasyonID INT;
BEGIN
    -- Rastgele bir Lokasyon ID'si oluştur
    rastgeleLokasyonID := floor(random() * 1000000)::INT; -- 0-999999 arası bir değer

    -- Lokasyonlar tablosuna kaydet, subeKodu'nu Kitaplar tablosundan al
    INSERT INTO Lokasyonlar (lokasyonID, SubeID, kitapID)
    VALUES (rastgeleLokasyonID, NEW.subeKodu, NEW.KitapID); -- subeKodu'nu Kitaplar tablosundan alıyoruz

    -- Kitaplar tablosundaki lokasyonID sütununu güncelle
    UPDATE Kitaplar SET lokasyonID = rastgeleLokasyonID WHERE KitapID = NEW.KitapID;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Kitap lokasyon trigger
CREATE TRIGGER kitap_lokasyon_random_trigger
AFTER INSERT ON Kitaplar
FOR EACH ROW EXECUTE FUNCTION kitap_lokasyon_random();


--kitap ödünç fonksiyonu
CREATE OR REPLACE FUNCTION kitap_odunc_verme(
    p_kitapID INT,
    p_uyeID INT
)
RETURNS VOID AS $$
BEGIN
    -- Kitabın durumunu false yap
    UPDATE Kitaplar
    SET durum = FALSE
    WHERE KitapID = p_kitapID;

    -- Islemler tablosuna ödünç işlemi ekle
    INSERT INTO Islemler (kitapID, uyeID, islemTuru)
    VALUES (p_kitapID, p_uyeID, 'ödünç');
    

END;
$$ LANGUAGE plpgsql;




CREATE OR REPLACE FUNCTION kitap_iade_verme(
    p_kitapID INT,
    p_uyeID INT
)
RETURNS VOID AS $$
BEGIN
    -- Kitabın durumunu true yap (iade işlemi)
    UPDATE Kitaplar
    SET durum = TRUE
    WHERE KitapID = p_kitapID;

    -- Islemler tablosuna iade işlemi ekle
    INSERT INTO Islemler (kitapID, uyeID, islemTuru)
    VALUES (p_kitapID, p_uyeID, 'iade');

END;


$$ LANGUAGE plpgsql;
 


CREATE OR REPLACE FUNCTION calisan_ekle(
    p_ad VARCHAR(255),
    p_soyad VARCHAR(255),
    p_telefonNo VARCHAR(11),
    p_mail VARCHAR(255),
    p_adres TEXT
)
RETURNS VOID AS $$
DECLARE
    v_kisiID INT;
BEGIN
    -- Kisiler tablosuna çalışan bilgilerini ekle
    INSERT INTO Kisiler (ad, soyad, telefonNo, mail, adres)
    VALUES (p_ad, p_soyad, p_telefonNo, p_mail, p_adres)
    RETURNING kisiID INTO v_kisiID;


    INSERT INTO Calisanlar (kisiID, baslamaTarihi)
    VALUES (v_kisiID, CURRENT_DATE);

    -- Fonksiyonun bitişi
    RETURN;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE calisan_sil(p_kisi_id INT)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Calisanlar tablosundan çalışanı sil
    DELETE FROM Calisanlar WHERE kisiID = p_kisi_id;

    -- Kisiler tablosundan kişiyi sil
    DELETE FROM Kisiler WHERE kisiID = p_kisi_id;
END;
$$;


CREATE OR REPLACE FUNCTION uye_ekle(
    p_ad VARCHAR(255),
    p_soyad VARCHAR(255),
    p_telefonNo VARCHAR(11),
    p_mail VARCHAR(255),
    p_adres TEXT
)
RETURNS VOID AS $$
DECLARE
    v_kisiID INT;
BEGIN
    -- Kisiler tablosuna üye bilgilerini ekle
    INSERT INTO Kisiler (ad, soyad, telefonNo, mail, adres)
    VALUES (p_ad, p_soyad, p_telefonNo, p_mail, p_adres)
    RETURNING kisiID INTO v_kisiID;

    -- Üye bilgilerini Uyeler tablosuna ekle ve üyelik tarihini otomatik olarak bugünün tarihi yap
    INSERT INTO Uyeler (kisiID, uyelikTarihi)
    VALUES (v_kisiID, CURRENT_DATE);

    -- Fonksiyonun bitişi
    RETURN;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE PROCEDURE uye_sil(p_kisi_id INT)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Üyeyi sil
    DELETE FROM Uyeler WHERE kisiID = p_kisi_id;

    -- Üyeye ait kişiyi sil
    DELETE FROM Kisiler WHERE kisiID = p_kisi_id;
END;
$$;


-- Kitap alma fonksiyonu
CREATE OR REPLACE FUNCTION kitap_alim_ekle()
RETURNS TRIGGER AS $$
BEGIN
    -- Kitap alım tarihini otomatik olarak bugünün tarihi olarak atar
    NEW.tarih := CURRENT_DATE;

    -- Kitap alımını Kitap_Alim tablosuna ekler
    INSERT INTO Kitap_Alim (tedarikciID, tarih, miktar, fiyat, kitapID, subeKodu)
    VALUES (NEW.tedarikciID, NEW.tarih, NEW.miktar, NEW.fiyat, NEW.kitapID, NEW.subeKodu);

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION bagis_ekle(
    p_uyeID INT,
    p_kitapAdeti INT,
    p_aciklama TEXT
)
RETURNS VOID AS $$
BEGIN
    -- Bagis tablosuna bağış kaydını ekleyelim
    INSERT INTO Bagis (uyeID, kitapAdeti, tarih, aciklama)
    VALUES (p_uyeID, p_kitapAdeti, CURRENT_DATE, p_aciklama);
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION bagis_aciklama_teşekkürler()
RETURNS TRIGGER AS $$
BEGIN
    -- Bağış yapılırken, aciklama sütununa 'Teşekkürler' yazılır
    NEW.aciklama := 'Teşekkürler';
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER bagis_aciklama_teşekkürler_trigger
BEFORE INSERT ON Bagis
FOR EACH ROW
EXECUTE FUNCTION bagis_aciklama_teşekkürler();


-- etkinlik ekleme fonksiyonu 
CREATE OR REPLACE FUNCTION etkinlik_ekle(
    p_calisanID INT,
    p_ad VARCHAR(255),
    p_tarih TIMESTAMP,
    p_aciklama TEXT
)
RETURNS VOID AS $$
BEGIN
    INSERT INTO Etkinlikler (calisanID, ad, tarih, aciklama)
    VALUES (p_calisanID, p_ad, p_tarih, p_aciklama);
    RAISE NOTICE 'Etkinlik başarıyla eklendi.';
END;
$$ LANGUAGE plpgsql;

--etkinlik listeleme fonksiyonu
CREATE OR REPLACE FUNCTION etkinlik_listele()
RETURNS TABLE (
    etkinlikID INT,
    calisanID INT,
    ad VARCHAR,
    tarih TIMESTAMP,
    aciklama TEXT
) AS $$
BEGIN
    RETURN QUERY SELECT e.etkinlikID, e.calisanID, e.ad, e.tarih, e.aciklama
                 FROM Etkinlikler e;
END;
$$ LANGUAGE plpgsql;

--açıklama triggerı
CREATE OR REPLACE FUNCTION ekle_ve_eklenti_aciklama()
RETURNS TRIGGER AS $$
BEGIN
    -- Açıklamaya otomatik ekleme işlemi
    NEW.aciklama := NEW.aciklama || ' || katılımlarınızı bekliyoruz... ';
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_aciklama_ekle
BEFORE INSERT ON Etkinlikler
FOR EACH ROW
EXECUTE FUNCTION ekle_ve_eklenti_aciklama();




CREATE OR REPLACE FUNCTION maliyet_hesapla(p_alimID INT)
RETURNS VOID AS $$
DECLARE
    v_fiyat NUMERIC(10, 2);
    v_miktar INT;
    v_toplamMaliyet NUMERIC(10, 2);
BEGIN
    -- Kitap alım bilgilerini al
    SELECT fiyat, miktar INTO v_fiyat, v_miktar
    FROM Kitap_Alim
    WHERE alimID = p_alimID;

    -- Toplam maliyeti hesapla
    v_toplamMaliyet := v_fiyat * v_miktar;

    -- Hesaplanan maliyeti Maliyet tablosuna ekle
    INSERT INTO Maliyet (alimID, toplamMaliyet)
    VALUES (p_alimID, v_toplamMaliyet);
END;
$$ LANGUAGE plpgsql;

SELECT * FROM kitaplar;


INSERT INTO Kisiler (ad, soyad, telefonNo, mail, adres) VALUES
('Orhan', 'Pamuk', '5311234567', 'orhan.pamuk@example.com', 'İstanbul, Türkiye'),
('Elif', 'Şafak', '5312234567', 'elif.safak@example.com', 'Strasbourg, Fransa'),
('Yaşar', 'Kemal', '5313234567', 'yasar.kemal@example.com', 'Adana, Türkiye'),
('Nazım', 'Hikmet', '5314234567', 'nazim.hikmet@example.com', 'Moskova, Rusya'),
('Ahmet', 'Hamdi Tanpınar', '5315234567', 'ahmet.hamdi@example.com', 'İstanbul, Türkiye'),
('Halide', 'Edib Adıvar', '5316234567', 'halide.edib@example.com', 'İstanbul, Türkiye'),
('Peyami', 'Safa', '5317234567', 'peyami.safa@example.com', 'İstanbul, Türkiye'),
('Sait', 'Faik Abasıyanık', '5318234567', 'sait.faik@example.com', 'Burgazada, Türkiye'),
('Sabahattin', 'Ali', '5319234567', 'sabahattin.ali@example.com', 'Edirne, Türkiye'),
('Oğuz', 'Atay', '5321234567', 'oguz.atay@example.com', 'İstanbul, Türkiye'),
('Haldun', 'Taner', '5322234567', 'haldun.taner@example.com', 'İstanbul, Türkiye'),
('İlber', 'Ortaylı', '5323234567', 'ilber.ortayli@example.com', 'Ankara, Türkiye'),
('Ayşe', 'Kulin', '5324234567', 'ayse.kulin@example.com', 'İstanbul, Türkiye'),
('Zülfü', 'Livaneli', '5325234567', 'zulfu.livaneli@example.com', 'Ankara, Türkiye'),
('Ahmet', 'Ümit', '5326234567', 'ahmet.umit@example.com', 'Gaziantep, Türkiye'),
('Murathan', 'Mungan', '5327234567', 'murathan.mungan@example.com', 'Mardin, Türkiye'),
('Tarık', 'Buğra', '5328234567', 'tarik.bugra@example.com', 'Akşehir, Türkiye'),
('Attilâ', 'İlhan', '5329234567', 'attila.ilhan@example.com', 'Menemen, Türkiye'),
('Refik', 'Halit Karay', '5331234567', 'refik.halit@example.com', 'İstanbul, Türkiye'),
('Kemal', 'Tahir', '5332234567', 'kemal.tahir@example.com', 'İstanbul, Türkiye'),
('Adalet', 'Ağaoğlu', '5333234567', 'adalet.agaoglu@example.com', 'Ankara, Türkiye'),
('Reşat', 'Nuri Güntekin', '5334234567', 'resat.nuri@example.com', 'İstanbul, Türkiye'),
('Orhan', 'Veli Kanık', '5335234567', 'orhan.veli@example.com', 'İstanbul, Türkiye'),
('Cemal', 'Süreya', '5336234567', 'cemal.sureya@example.com', 'Erzincan, Türkiye'),
('Edip', 'Cansever', '5337234567', 'edip.cansever@example.com', 'İstanbul, Türkiye'),
('Turgut', 'Uyar', '5338234567', 'turgut.uyar@example.com', 'Ankara, Türkiye'),
('Tomris', 'Uyar', '5339234567', 'tomris.uyar@example.com', 'İstanbul, Türkiye'),
('Latife', 'Tekin', '5341234567', 'latife.tekin@example.com', 'Kayseri, Türkiye'),
('Gülten', 'Dayıoğlu', '5342234567', 'gulten.dayioglu@example.com', 'Kütahya, Türkiye'),
('İhsan', 'Oktay Anar', '5343234567', 'ihsan.oktay@example.com', 'Yozgat, Türkiye'),
('Yılmaz', 'Günay', '5344234567', 'yilmaz.gunay@example.com', 'Adana, Türkiye'),
('Aziz', 'Nesin', '5345234567', 'aziz.nesin@example.com', 'İstanbul, Türkiye'),
('Ferit', 'Edgü', '5346234567', 'ferit.edgu@example.com', 'İstanbul, Türkiye'),
('Mehmet', 'Rauf', '5347234567', 'mehmet.rauf@example.com', 'İstanbul, Türkiye'),
('Hüseyin', 'Rahmi Gürpınar', '5348234567', 'huseyin.rahmi@example.com', 'İstanbul, Türkiye'),
('Ahmet', 'Rasim', '5349234567', 'ahmet.rasim@example.com', 'İstanbul, Türkiye'),
('Beşir', 'Fuat', '5351234567', 'besir.fuat@example.com', 'İstanbul, Türkiye'),
('Yakup Kadri', 'Karaosmanoğlu', '5352234567', 'yakup.kadri@example.com', 'Kahire, Mısır'),
('Halit', 'Ziya Uşaklıgil', '5353234567', 'halit.ziya@example.com', 'İstanbul, Türkiye'),
('Cevat', 'Şakir Kabaağaçlı', '5354234567', 'cevat.sakir@example.com', 'Bodrum, Türkiye'),
('Necip', 'Fazıl Kısakürek', '5355234567', 'necip.fazil@example.com', 'İstanbul, Türkiye'),
('Sezai', 'Karakoç', '5356234567', 'sezai.karakoc@example.com', 'Diyarbakır, Türkiye'),
('Ece', 'Temelkuran', '5357234567', 'ece.temelkuran@example.com', 'İzmir, Türkiye'),
('Perihan', 'Mağden', '5358234567', 'perihan.magden@example.com', 'İstanbul, Türkiye'),
('Leyla', 'Erbil', '5359234567', 'leyla.erbil@example.com', 'İstanbul, Türkiye'),
('Şule', 'Yüksel Şenler', '5361234567', 'sule.senler@example.com', 'İstanbul, Türkiye'),
('Füruzan', 'Füruzan', '5362234567', 'furu.furuzan@example.com', 'İstanbul, Türkiye'),
('Emine', 'Işınsu', '5363234567', 'emine.isinsu@example.com', 'Ankara, Türkiye');





INSERT INTO Yazarlar (kisiID) VALUES (1), (2), (3), (4), (5), (6), (7), (8), (9), (10), (11), (12), (13), (14), (15), (16), (17), (18), (19), (20), (21), (22), (23), (24), (25), (26), (27), (28), (29), (30), (31), (32), (33), (34), (35), (36), (37), (38), (39), (40), (41), (42), (43), (44), (45), (46), (47),(48);



INSERT INTO Yayinevi (ad, iletisimBilgisi) VALUES
('İthaki Yayınları', 'Tel: 0212 232 21 22, Email: info@ithakiyayinlari.com'),
('Can Yayınları', 'Tel: 0212 292 88 88, Email: can@canyayinlari.com'),
('Everest Yayınları', 'Tel: 0212 232 16 62, Email: everest@everestyayinlari.com'),
('Doğan Kitap', 'Tel: 0212 335 53 53, Email: info@dogankitap.com.tr'),
('Sel Yayıncılık', 'Tel: 0212 251 11 25, Email: info@selyayincilik.com'),
('Kültür Yayınları', 'Tel: 0212 221 52 30, Email: kultur@kultur.com.tr'),
('Penguin Books', 'Tel: +44 20 7133 9000, Email: penguin@penguin.co.uk'),
('HarperCollins', 'Tel: +44 20 8273 2300, Email: info@harpercollins.co.uk'),
('Alfa Yayınları', 'Tel: 0212 212 42 65, Email: info@alfakitap.com'),
('Timaş Yayınları', 'Tel: 0212 674 90 90, Email: timas@timas.com.tr');


-- Kitap Kategorileri Tablosu
INSERT INTO Kitap_Kategorileri (ad) VALUES
('Roman'),
('Klasikler'),
('Bilim Kurgu'),
('Felsefe'),
('Tarih'),
('Biyografi'),
('Çocuk Kitapları'),
('Hikaye'),
('Edebiyat'),
('Kişisel Gelişim'),
('Sanat'),
('Poetika'),
('Politika'),
('Edebiyat Kuramı'),
('Psikoloji');


INSERT INTO Subeler (ad, adres, telefon) VALUES
('Ana Şube', 'Beyoğlu, İstiklal Caddesi', '02121234567'),
('Kadıköy Şube', 'Bağdat Caddesi', '02162345678');


INSERT INTO Kitaplar (ad, yazarID, yayinEviID, kategoriID, yayinYili, subeKodu) VALUES
('Masumiyet Müzesi', 1, 7, 9, 2008, 1),
('Benim Adım Kırmızı', 1, 2, 1, 1998, 2),
('Aşk', 2, 6, 9, 2009, 1),
('İskender', 2, 3, 9, 2011, 2),
('İnce Memed', 3, 4, 1, 1955, 1),
('Yer Demir Gök Bakır', 3, 1, 1, 1963, 2),
('Kürk Mantolu Madonna', 4, 9, 1, 1943, 1),
('İçimizdeki Şeytan', 4, 8, 14, 1940, 2),
('Tutunamayanlar', 5, 3, 13, 1971, 1),
('Tehlikeli Oyunlar', 5, 5, 9, 1973, 2),
('Saatleri Ayarlama Enstitüsü', 6, 2, 9, 1961, 1),
('Huzur', 6, 7, 14, 1949, 2),
('Sinekli Bakkal', 7, 6, 8, 1936, 1),
('Ateşten Gömlek', 7, 8, 5, 1923, 2),
('Fatih Harbiye', 8, 4, 1, 1931, 1),
('Dokuzuncu Hariciye Koğuşu', 8, 3, 14, 1930, 2),
('Semaver', 9, 1, 8, 1936, 1),
('Lüzumsuz Adam', 9, 7, 8, 1948, 2),
('Çalıkuşu', 10, 5, 1, 1922, 1),
('Yaprak Dökümü', 10, 2, 9, 1930, 2),
('Bir Düğün Gecesi', 11, 6, 9, 1979, 1),
('Fikrimin İnce Gülü', 11, 8, 8, 1976, 2),
('Anadolu Notları', 12, 4, 5, 1952, 1),
('Yakup Kadri', 12, 9, 1, 1941, 2),
('Cemile', 13, 1, 9, 1968, 1),
('Yorgun Savaşçı', 13, 10, 5, 1965, 2),
('Taaşşuk-u Talat ve Fitnat', 14, 3, 2, 1872, 1),
('Araba Sevdası', 15, 2, 2, 1898, 2),
('Aşk-ı Memnu', 15, 7, 2, 1899, 1),
('Bodrum Masalı', 16, 5, 8, 1946, 2),
('Kırık Hayatlar', 16, 8, 9, 1923, 1),
('Çocuk Kalbi', 17, 6, 7, 1979, 2),
('Bir Gün Tek Başına', 18, 9, 1, 1974, 1),
('Serenad', 19, 4, 14, 2011, 2),
('Leyla ile Mecnun', 19, 3, 1, 2006, 1),
('Cevdet Bey ve Oğulları', 20, 2, 9, 1982, 2),
('Kafamda Bir Tuhaflık', 20, 1, 9, 2014, 1),
('Puslu Kıtalar Atlası', 21, 10, 3, 1995, 2),
('Efrasiyab’ın Hikâyeleri', 21, 7, 8, 1998, 1),
('Anayurt Oteli', 22, 8, 14, 1973, 2),
('Kumral Ada Mavi Tuna', 23, 6, 1, 1997, 1),
('Mahur Beste', 24, 9, 9, 1944, 2),
('Kar', 25, 5, 13, 2002, 1),
('Kayıp Söz', 25, 3, 12, 2010, 2),
('Yeni Hayat', 26, 2, 1, 1994, 1),
('Sessiz Ev', 26, 1, 9, 1983, 2),
('Aylak Adam', 27, 4, 14, 1959, 1),
('Tutkulu Perçem', 28, 6, 12, 1955, 2),
('Kör Baykuş', 28, 10, 14, 1976, 1);



INSERT INTO Tedarikci (ad, iletisim) VALUES
('Ekim Matbaa', 'Telefon: 0212 666 2233, E-posta: info@ekimmatbaa.com, Adres: İstanbul, Beyoğlu, Tünel Caddesi No: 15'),
('Kendirli Matbaa', 'Telefon: 0216 233 4455, E-posta: info@kendirlimatbaa.com, Adres: İstanbul, Üsküdar, Çamlık Caddesi No: 22'),
('Baskı Kitabevi', 'Telefon: 0212 498 6754, E-posta: info@baskikitabevi.com, Adres: İstanbul, Fatih, Vatan Caddesi No: 35'),
('Vega Matbaa', 'Telefon: 0216 366 7890, E-posta: info@vegametbaa.com, Adres: İstanbul, Kartal, Huzur Mahallesi No: 10'),
('Profi Matbaa', 'Telefon: 0212 723 5678, E-posta: profi.matbaa@gmail.com, Adres: İstanbul, Bağcılar, Mahmutbey Caddesi No: 40'),
('Taksim Matbaa', 'Telefon: 0212 458 2345, E-posta: taksim.matbaa@outlook.com, Adres: İstanbul, Beyoğlu, İstiklal Caddesi No: 99'),
('Karma Matbaa', 'Telefon: 0216 553 9988, E-posta: info@karmamatbaa.com, Adres: İstanbul, Pendik, Kaynarca Caddesi No: 25'),
('Alfa Matbaa', 'Telefon: 0210 432 9876, E-posta: info@alfamatbaa.com, Adres: İstanbul, Esenler, Mahir İz Caddesi No: 50'),
('Elite Matbaa', 'Telefon: 0212 444 3322, E-posta: elite.matbaa@gmail.com, Adres: İstanbul, Şişli, Nişantaşı Mahallesi No: 10'),
('Matbaa Dünyası', 'Telefon: 0216 678 9012, E-posta: info@matbaadunyasi.com, Adres: İstanbul, Kadıköy, Fikirtepe Mahallesi No: 5');

SELECT * FROM uyeler;