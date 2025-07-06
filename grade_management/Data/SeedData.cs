// using grade_management.Models;

// namespace grade_management.Data
// {
//     public static class SeedData
//     {
//         public static async Task SeedAsync(ApplicationDbContext context)
//         {
//             if (context.Database.EnsureCreated())
//             {
//                 // Seed Khoa
//                 if (!context.Khoas.Any())
//                 {
//                     var khoas = new List<KhoaModel>
//                     {
//                         new KhoaModel
//                         {
//                             MaKhoa = "CNTT",
//                             TenKhoa = "Công nghệ thông tin",
//                             TruongKhoa = "TS. Nguyễn Văn A",
//                             Email = "cntt@university.edu.vn",
//                             SoDienThoai = "0123456789",
//                             NamThanhLap = 2000,
//                             TrangThai = true
//                         },
//                         new KhoaModel
//                         {
//                             MaKhoa = "KT",
//                             TenKhoa = "Kế toán",
//                             TruongKhoa = "TS. Trần Thị B",
//                             Email = "ketoan@university.edu.vn",
//                             SoDienThoai = "0987654321",
//                             NamThanhLap = 1995,
//                             TrangThai = true
//                         }
//                     };

//                     context.Khoas.AddRange(khoas);
//                     await context.SaveChangesAsync();
//                 }

//                 // Seed LopHoc
//                 if (!context.LopHocs.Any())
//                 {
//                     var lopHocs = new List<LopHocModel>
//                     {
//                         new LopHocModel
//                         {
//                             MaLop = "CNTT01",
//                             TenLop = "Công nghệ thông tin 01",
//                             MaKhoa = "CNTT",
//                             NamNhapHoc = 2020,
//                             NienKhoa = "2020-2024",
//                             SiSoToiDa = 50
//                         },
//                         new LopHocModel
//                         {
//                             MaLop = "CNTT02",
//                             TenLop = "Công nghệ thông tin 02",
//                             MaKhoa = "CNTT",
//                             NamNhapHoc = 2020,
//                             NienKhoa = "2020-2024",
//                             SiSoToiDa = 50
//                         },
//                         new LopHocModel
//                         {
//                             MaLop = "KT01",
//                             TenLop = "Kế toán 01",
//                             MaKhoa = "KT",
//                             NamNhapHoc = 2020,
//                             NienKhoa = "2020-2024",
//                             SiSoToiDa = 45
//                         }
//                     };

//                     context.LopHocs.AddRange(lopHocs);
//                     await context.SaveChangesAsync();
//                 }

//                 // Seed SinhVien
//                 if (!context.SinhViens.Any())
//                 {
//                     var sinhViens = new List<SinhVienModel>
//                     {
//                         new SinhVienModel
//                         {
//                             MaSinhVien = "SV001",
//                             HoTen = "Nguyễn Văn An",
//                             NgaySinh = new DateTime(2000, 1, 15),
//                             GioiTinh = true,
//                             Email = "anvn@email.com",
//                             SoDienThoai = "0123456789",
//                             DiaChi = "Hà Nội",
//                             MaLop = "CNTT01",
//                             NamNhapHoc = 2020,
//                             TrangThai = TrangThaiSinhVien.DangHoc
//                         },
//                         new SinhVienModel
//                         {
//                             MaSinhVien = "SV002",
//                             HoTen = "Trần Thị Bình",
//                             NgaySinh = new DateTime(2000, 5, 20),
//                             GioiTinh = false,
//                             Email = "binhtt@email.com",
//                             SoDienThoai = "0987654321",
//                             DiaChi = "Hồ Chí Minh",
//                             MaLop = "CNTT01",
//                             NamNhapHoc = 2020,
//                             TrangThai = TrangThaiSinhVien.DangHoc
//                         },
//                         new SinhVienModel
//                         {
//                             MaSinhVien = "SV003",
//                             HoTen = "Lê Văn Cường",
//                             NgaySinh = new DateTime(1999, 12, 10),
//                             GioiTinh = true,
//                             Email = "cuonglv@email.com",
//                             SoDienThoai = "0111222333",
//                             DiaChi = "Đà Nẵng",
//                             MaLop = "CNTT02",
//                             NamNhapHoc = 2020,
//                             TrangThai = TrangThaiSinhVien.DangHoc
//                         }
//                     };

//                     context.SinhViens.AddRange(sinhViens);
//                     await context.SaveChangesAsync();
//                 }

//                 // Seed MonHoc
//                 if (!context.MonHocs.Any())
//                 {
//                     var monHocs = new List<MonHocModel>
//                     {
//                         new MonHocModel
//                         {
//                             MaMonHoc = "LTTQ",
//                             TenMonHoc = "Lập trình truyền thống",
//                             SoTinChi = 3,
//                             SoTietLyThuyet = 30,
//                             SoTietThucHanh = 15,
//                             MaKhoa = "CNTT",
//                             LoaiMonHoc = LoaiMonHoc.BatBuoc,
//                             HocKy = 1,
//                             TrangThai = true
//                         },
//                         new MonHocModel
//                         {
//                             MaMonHoc = "CSDL",
//                             TenMonHoc = "Cơ sở dữ liệu",
//                             SoTinChi = 3,
//                             SoTietLyThuyet = 30,
//                             SoTietThucHanh = 15,
//                             MaKhoa = "CNTT",
//                             LoaiMonHoc = LoaiMonHoc.BatBuoc,
//                             HocKy = 2,
//                             TrangThai = true
//                         },
//                         new MonHocModel
//                         {
//                             MaMonHoc = "KTT",
//                             TenMonHoc = "Kế toán tài chính",
//                             SoTinChi = 4,
//                             SoTietLyThuyet = 45,
//                             SoTietThucHanh = 15,
//                             MaKhoa = "KT",
//                             LoaiMonHoc = LoaiMonHoc.BatBuoc,
//                             HocKy = 1,
//                             TrangThai = true
//                         }
//                     };

//                     context.MonHocs.AddRange(monHocs);
//                     await context.SaveChangesAsync();
//                 }
//             }
//         }
//     }
// } 