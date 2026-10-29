using JLM.Models;
using Microsoft.AspNetCore.Identity;

namespace JLM.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@jlmstore.com.br";
        var adminPassword = configuration["Seed:AdminPassword"] ?? "Jlm@2026";
        var customerEmail = configuration["Seed:CustomerEmail"] ?? "cliente@jlmstore.com.br";
        var customerPassword = configuration["Seed:CustomerPassword"] ?? "Jlm@2026";

        if (!db.Users.Any())
        {
            var admin = new User
            {
                FullName = "João Laumir",
                Email = adminEmail,
                Role = "Admin",
                Phone = "(11) 99999-0001",
                City = "São Paulo",
                State = "SP",
                CreatedAt = DateTime.UtcNow.AddMonths(-8)
            };
            admin.PasswordHash = passwordHasher.HashPassword(admin, adminPassword);

            var customer = new User
            {
                FullName = "Rafael Moreira",
                Email = customerEmail,
                Role = "Customer",
                Phone = "(11) 98888-2211",
                Cpf = "123.456.789-00",
                Address = "Rua dos Pinheiros",
                AddressNumber = "842",
                Complement = "Apto 72",
                Neighborhood = "Pinheiros",
                City = "São Paulo",
                State = "SP",
                ZipCode = "05422-001",
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            };
            customer.PasswordHash = passwordHasher.HashPassword(customer, customerPassword);

            db.Users.AddRange(admin, customer);
            await db.SaveChangesAsync();
        }

        if (!db.Categories.Any())
        {
            db.Categories.AddRange(
                new Category
                {
                    Name = "Roupas",
                    Slug = "roupas",
                    Eyebrow = "Vestuário",
                    Description = "Alfaiataria, streetwear e essenciais construídos para durar. Cortes precisos, tecidos nobres e presença imediata.",
                    ImageUrl = "/images/products/camiseta-oversized-signature.svg",
                    SortOrder = 1
                },
                new Category
                {
                    Name = "Joias",
                    Slug = "joias",
                    Eyebrow = "Alta joalheria",
                    Description = "Correntes, anéis e pulseiras em ouro 18k e prata 925. Peças masculinas de acabamento impecável.",
                    ImageUrl = "/images/products/corrente-cubana-ouro-18k.svg",
                    SortOrder = 2
                },
                new Category
                {
                    Name = "Acessórios",
                    Slug = "acessorios",
                    Eyebrow = "Detalhes que definem",
                    Description = "Óculos, relógios, couro e calçados. O detalhe final que transforma um look em assinatura.",
                    ImageUrl = "/images/products/oculos-de-sol-laumir-gold.svg",
                    SortOrder = 3
                });
        }

        if (!db.Collections.Any())
        {
            db.Collections.AddRange(
                new Collection
                {
                    Name = "Rua & Presença",
                    Slug = "rua-e-presenca",
                    Tagline = "Streetwear de alto padrão",
                    Description = "O encontro entre o streetwear e a alfaiataria. Oversized com estrutura, preto profundo e o dourado da JLM como assinatura. Peças pensadas para quem ocupa espaço sem precisar levantar a voz.",
                    ImageUrl = "/images/products/moletom-oversized-presenca.svg",
                    IsFeatured = true,
                    SortOrder = 1
                },
                new Collection
                {
                    Name = "Noite Dourada",
                    Slug = "noite-dourada",
                    Tagline = "Joias e presença",
                    Description = "Ouro 18k, onyx e brilho controlado. Uma linha para jantares, eventos e noites em que a única regra é ser lembrado. Sofisticação sem excesso.",
                    ImageUrl = "/images/products/corrente-cubana-ouro-18k.svg",
                    IsFeatured = true,
                    SortOrder = 2
                },
                new Collection
                {
                    Name = "Essenciais JLM",
                    Slug = "essenciais-jlm",
                    Tagline = "A base de todo estilo",
                    Description = "Camisetas de algodão pesado, alfaiataria impecável e couro legítimo. O básico elevado ao padrão JLM — peças que conversam entre si e atravessam estações.",
                    ImageUrl = "/images/products/camiseta-boxy-heavy-cotton.svg",
                    IsFeatured = true,
                    SortOrder = 3
                },
                new Collection
                {
                    Name = "Edição Laumir",
                    Slug = "edicao-laumir",
                    Tagline = "Limitada. Assinada.",
                    Description = "Lançamentos em tiragem limitada, assinados pelo mascote e fundador da JLM. Peças numeradas, acabamento artesanal e presença de colecionador.",
                    ImageUrl = "/images/products/blazer-premium-gold-line.svg",
                    IsFeatured = true,
                    SortOrder = 4
                });
        }

        await db.SaveChangesAsync();

        if (!db.Products.Any())
        {
            var categories = db.Categories.ToDictionary(c => c.Slug);
            var collections = db.Collections.ToDictionary(c => c.Slug);

            var products = new List<Product>
            {
                NewProduct("Camiseta Oversized Signature", "camiseta-oversized-signature", "camiseta-oversized-signature",
                    "Algodão pesado 240g com caimento oversized e monograma dourado.",
                    "A peça que inaugurou a JLM. Confeccionada em algodão penteado de 240g, a Camiseta Oversized Signature tem caimento amplo, ombros caídos e gola dupla reforçada. O monograma JLM é aplicado em silk metálico dourado, com acabamento que resiste a lavagens.\n\nModelagem unissex com intenção masculina. Ideal para composições minimalistas ou sobreposições com alfaiataria.",
                    "100% algodão penteado 240g. Silk metálico dourado.", "Lavar do avesso em água fria. Não usar alvejante. Secar à sombra.",
                    189.90m, 239.90m, "JLM-TEE-001", 48, "roupas", "rua-e-presenca", 4.9m, 127,
                    true, true, true, "P,M,G,GG,XG", "Preto:#101010,Off-white:#EAE3D6", 1, 3),
                NewProduct("Camiseta Boxy Heavy Cotton", "camiseta-boxy-heavy-cotton", "camiseta-boxy-heavy-cotton",
                    "Corte boxy em off-white, algodão encorpado e acabamento premium.",
                    "Corte boxy, ombro estruturado e comprimento encurtado. A Camiseta Boxy Heavy Cotton utiliza algodão cardado de 260g, com toque seco e estrutura que mantém a silhueta ao longo do dia.\n\nO tom off-white é tingido em pequenos lotes, criando variações sutis a cada produção.",
                    "100% algodão cardado 260g. Tingimento em lote.", "Lavagem delicada a frio. Secagem natural.",
                    199.90m, null, "JLM-TEE-002", 36, "roupas", "essenciais-jlm", 4.8m, 84,
                    false, true, false, "P,M,G,GG", "Off-white:#EAE3D6,Areia:#C9BBA4", 2, 2),
                NewProduct("Moletom Oversized Presença", "moletom-oversized-presenca", "moletom-oversized-presenca",
                    "Moletom 420g com capuz duplo e bordado dourado. Presença em silêncio.",
                    "O Moletom Oversized Presença é a tradução literal do slogan da marca. Moletom peluciado de 420g, capuz duplo, bolso canguru amplo e punhos reforçados. O bordado JLM em dourado é aplicado no peito, com acabamento de alta densidade.\n\nSilhueta ampla e estruturada, sem excesso de volume.",
                    "80% algodão, 20% poliéster. Moletom peluciado 420g. Bordado dourado.", "Lavar do avesso. Não secar em máquina.",
                    389.90m, 449.90m, "JLM-HOO-001", 27, "roupas", "rua-e-presenca", 4.9m, 96,
                    true, true, true, "P,M,G,GG,XG", "Preto:#101010,Grafite:#292929", 3, 1),
                NewProduct("Jaqueta Bomber Urban Gold", "jaqueta-bomber-urban-gold", "jaqueta-bomber-urban-gold",
                    "Bomber em nylon premium com forro acetinado e zíper dourado.",
                    "Inspirada nas noites paulistanas, a Jaqueta Bomber Urban Gold combina nylon de alta densidade e forro acetinado. Zíper metálico dourado, bolsos internos e punhos em tricot. Uma peça de transição, feita para sobrepor camisetas e camisas.\n\nCaimento regular com leve folga no corpo.",
                    "Exterior 100% nylon. Forro 100% poliéster. Zíper metálico.", "Limpeza a seco. Não utilizar máquina de lavar.",
                    549.90m, 649.90m, "JLM-JKT-001", 18, "roupas", "rua-e-presenca", 4.8m, 43,
                    true, false, true, "P,M,G,GG", "Grafite:#292929,Preto:#101010", 4, 2),
                NewProduct("Camisa Linho Noir", "camisa-linho-noir", "camisa-linho-noir",
                    "Linho premium com toque macio e caimento fluido. Elegância sem esforço.",
                    "Camisa de linho premium com mistura de algodão para reduzir o amassado natural do tecido. Gola italiana, botões em madrepérola escura e acabamento em ponto duplo.\n\nPerfeita para dias quentes e noites de verão, aberta sobre camiseta ou fechada com alfaiataria.",
                    "55% linho, 45% algodão. Botões de madrepérola escura.", "Lavar a frio. Passar em temperatura média.",
                    299.90m, null, "JLM-SHT-001", 32, "roupas", "essenciais-jlm", 4.7m, 61,
                    false, false, true, "P,M,G,GG", "Preto:#101010,Off-white:#EAE3D6", 5, 3),
                NewProduct("Calça Cargo Utility", "calca-cargo-utility", "calca-cargo-utility",
                    "Cargo de sarja encorpada com bolsos utilitários e barra ajustável.",
                    "Sarja de algodão com elastano para mobilidade. Seis bolsos, sendo dois laterais com fechamento magnético e barra com cordão interno para ajuste de comprimento.\n\nO equilíbrio entre utilidade e sofisticação urbana.",
                    "97% algodão, 3% elastano. Sarja 320g.", "Lavar do avesso em água fria. Não usar secadora.",
                    329.90m, 399.90m, "JLM-PNT-001", 24, "roupas", "rua-e-presenca", 4.8m, 55,
                    false, true, false, "38,40,42,44,46", "Grafite:#292929,Preto:#101010", 6, 1),
                NewProduct("Calça Alfaiataria Slim", "calca-alfaiataria-slim", "calca-alfaiataria-slim",
                    "Alfaiataria com toque de lã fria, corte slim e cós confortável.",
                    "Calça de alfaiataria em lã fria com elastano, cós ajustável internamente e caimento slim sem apertar. Ideal para composições formais ou para quebrar o protocolo com tênis e camiseta.\n\nAcabamento com passador duplo e bolsos italianos.",
                    "68% poliéster, 30% viscose, 2% elastano. Lã fria.", "Limpeza a seco recomendada. Passar em temperatura baixa.",
                    349.90m, null, "JLM-PNT-002", 21, "roupas", "noite-dourada", 4.9m, 38,
                    true, false, false, "38,40,42,44,46", "Preto:#101010,Grafite:#292929", 7, 2),
                NewProduct("Camiseta Longline Essential", "camiseta-longline-essential", "camiseta-longline-essential",
                    "Comprimento longline, algodão macio e caimento reto.",
                    "Camiseta longline em algodão peruano de 220g. Comprimento estendido, gola canelada e acabamento limpo. A base perfeita para sobreposições com jaquetas e blazers.\n\nPeça essencial da linha Essentials JLM.",
                    "100% algodão peruano 220g.", "Lavar a frio. Secar à sombra.",
                    169.90m, 199.90m, "JLM-TEE-003", 52, "roupas", "essenciais-jlm", 4.6m, 72,
                    false, false, false, "P,M,G,GG,XG", "Off-white:#EAE3D6,Preto:#101010", 8, 3),
                NewProduct("Blazer Premium Gold Line", "blazer-premium-gold-line", "blazer-premium-gold-line",
                    "Blazer desconstruído com forro dourado e lapela estruturada.",
                    "O blazer que assina a Edição Laumir. Modelagem desconstruída, ombro natural, lapela em cetim e forro interno com monograma dourado. Botões metálicos banhados a ouro.\n\nUma peça de presença para ocasiões que exigem mais do que formalidade.",
                    "Lã fria premium com forro em cetim.", "Limpeza a seco exclusivamente.",
                    899.90m, 1099.90m, "JLM-BLZ-001", 12, "roupas", "edicao-laumir", 5.0m, 29,
                    true, true, false, "48,50,52,54", "Preto:#101010,Grafite:#292929", 9, 1),
                NewProduct("Conjunto Streetwear JLM", "conjunto-streetwear-jlm", "conjunto-streetwear-jlm",
                    "Conjunto de moletom e calça em grafite com detalhes dourados.",
                    "Conjunto streetwear composto por moletom e calça jogger. Moletom de 400g com capuz e bolso canguru; calça com punho elástico e bolsos laterais. Detalhes em dourado no cordão e no monograma.\n\nVendido como conjunto, em embalagem presenteável JLM.",
                    "80% algodão, 20% poliéster.", "Lavar do avesso em água fria. Secar à sombra.",
                    649.90m, 749.90m, "JLM-SET-001", 15, "roupas", "edicao-laumir", 4.9m, 34,
                    true, true, true, "P,M,G,GG", "Grafite:#292929,Preto:#101010", 10, 2),

                NewProduct("Corrente Cubana Ouro 18k", "corrente-cubana-ouro-18k", "corrente-cubana-ouro-18k",
                    "Elos cubanos em ouro 18k, fecho reforçado e brilho controlado.",
                    "A peça central da linha Noite Dourada. Corrente de elos cubanos em ouro 18k, com 6mm de espessura e fecho reforçado. Acabamento polido que reflete luz sem excesso.\n\nAcompanha estojo JLM, certificado de autenticidade e polidor de joias.",
                    "Ouro 18k (750). Comprimento 55cm. Largura 6mm.", "Evitar contato com perfumes e cloro. Limpar com flanela macia.",
                    2499.90m, 2899.90m, "JLM-JWL-001", 8, "joias", "noite-dourada", 5.0m, 41,
                    true, true, true, "Único", "Ouro 18k:#C9A45C", 11, 1),
                NewProduct("Pingente JLM Monograma", "pingente-jlm-monograma", "pingente-jlm-monograma",
                    "Monograma JLM em ouro 18k com acabamento escovado. Corrente inclusa.",
                    "Pingente retangular com o monograma JLM em relevo, confeccionado em ouro 18k com acabamento escovado. Acompanha corrente de elos venezianos de 60cm.\n\nUma assinatura discreta, feita para ser usada todos os dias.",
                    "Ouro 18k (750). Pingente 3,2cm. Corrente 60cm inclusa.", "Limpar com flanela seca. Guardar separadamente.",
                    1899.90m, null, "JLM-JWL-002", 10, "joias", "edicao-laumir", 4.9m, 26,
                    true, true, false, "Único", "Ouro 18k:#C9A45C", 12, 2),
                NewProduct("Pulseira Elos Cubanos", "pulseira-elos-cubanos", "pulseira-elos-cubanos",
                    "Pulseira de elos cubanos em ouro 18k com fecho de segurança.",
                    "Pulseira masculina de elos cubanos em ouro 18k, 5mm de largura e fecho de segurança duplo. O complemento natural da Corrente Cubana.\n\nAcabamento polido e conforto para uso diário.",
                    "Ouro 18k (750). Comprimento 21cm. Largura 5mm.", "Evitar contato com produtos químicos. Limpar com flanela.",
                    1299.90m, 1499.90m, "JLM-JWL-003", 14, "joias", "noite-dourada", 4.9m, 33,
                    false, false, true, "Único", "Ouro 18k:#C9A45C", 13, 1),
                NewProduct("Anel Signature Onyx", "anel-signature-onyx", "anel-signature-onyx",
                    "Prata 925 banhada a ouro com pedra onyx negra.",
                    "Anel de prata 925 com banho de ouro 18k e pedra onyx negra lapidada. Design assimétrico, feito para o dedo indicador ou anelar.\n\nUma peça de contraste: o negro profundo da pedra contra o dourado da bandagem.",
                    "Prata 925 com banho de ouro 18k. Onyx natural.", "Retirar antes de atividades físicas. Guardar no estojo.",
                    599.90m, 699.90m, "JLM-JWL-004", 20, "joias", "edicao-laumir", 4.8m, 19,
                    true, false, false, "16,18,20,22,24", "Preto:#101010", 14, 2),
                NewProduct("Anel Duplo Bold", "anel-duplo-bold", "anel-duplo-bold",
                    "Duas bandas em ouro 18k com acabamento martelado.",
                    "Anel de duas bandas sobrepostas em ouro 18k, com acabamento martelado manualmente. Cada peça é única — as marcas do martelo variam de anel para anel.\n\nPresença no dedo, sutileza no detalhe.",
                    "Ouro 18k (750). Acabamento martelado manual.", "Limpar com flanela macia. Evitar impactos.",
                    1590.90m, null, "JLM-JWL-005", 9, "joias", "edicao-laumir", 5.0m, 12,
                    false, true, false, "18,20,22,24", "Ouro 18k:#C9A45C", 15, 1),
                NewProduct("Brinco Argola Bold", "brinco-argola-bold", "brinco-argola-bold",
                    "Argola em ouro 18k com 4,5cm de diâmetro. Vendida em par.",
                    "Argola masculina em ouro 18k, com 4,5cm de diâmetro e fecho de pressão. Vendida em par, com acabamento polido e peso equilibrado para uso prolongado.",
                    "Ouro 18k (750). Diâmetro 4,5cm. Fecho de pressão.", "Evitar contato com água e perfumes.",
                    899.90m, 999.90m, "JLM-JWL-006", 16, "joias", "noite-dourada", 4.8m, 21,
                    false, false, false, "Único", "Ouro 18k:#C9A45C", 16, 3),
                NewProduct("Colar Cordão Vibe", "colar-cordao-vibe", "colar-cordao-vibe",
                    "Cordão trançado em ouro 18k com fecho ajustável.",
                    "Cordão trançado à mão em ouro 18k, com fecho ajustável de 45 a 55cm. Uma alternativa mais leve à corrente cubana, com o mesmo padrão de acabamento.\n\nIdeal para uso diário e sobreposições.",
                    "Ouro 18k (750). Comprimento ajustável 45–55cm.", "Guardar separado de outras joias. Limpar com flanela.",
                    1199.90m, null, "JLM-JWL-007", 13, "joias", "noite-dourada", 4.9m, 18,
                    false, false, true, "Único", "Ouro 18k:#C9A45C", 17, 2),
                NewProduct("Bracelete Couro & Ouro", "bracelete-couro-ouro", "bracelete-couro-ouro",
                    "Couro legítimo trançado com fecho em ouro 18k.",
                    "Bracelete em couro legítimo trançado, com fecho magnético banhado a ouro 18k e detalhe gravado JLM. O encontro entre o artesanal e o sofisticado.\n\nAjustável ao pulso, com acabamento resistente à água.",
                    "Couro legítimo. Fecho em ouro 18k. Ajustável 18–22cm.", "Evitar contato prolongado com água. Hidratar o couro ocasionalmente.",
                    449.90m, 529.90m, "JLM-JWL-008", 25, "joias", "rua-e-presenca", 4.7m, 27,
                    false, true, false, "Único", "Marrom:#4A3A2E,Preto:#101010", 18, 2),

                NewProduct("Óculos de Sol Laumir Gold", "oculos-de-sol-laumir-gold", "oculos-de-sol-laumir-gold",
                    "O óculos do mascote. Acetato preto, lentes G15 e detalhes dourados.",
                    "O modelo assinado por João Laumir. Armação em acetato italiano preto, lentes G15 com proteção UV400 e hastes com detalhe dourado JLM. Formato que valoriza o rosto sem exageros.\n\nAcompanha estojo rígido, flanela e certificado de autenticidade.",
                    "Acetato italiano. Lentes G15 UV400. Hastes metálicas.", "Limpar com flanela própria. Guardar no estojo.",
                    659.90m, 799.90m, "JLM-ACC-001", 22, "acessorios", "edicao-laumir", 5.0m, 58,
                    true, true, true, "Único", "Preto:#101010,Tartaruga:#6B4A2E", 19, 1),
                NewProduct("Relógio Chrono Noir", "relogio-chrono-noir", "relogio-chrono-noir",
                    "Movimento suíço, caixa preta e mostrador dourado. 5 ATM.",
                    "Relógio com movimento suíço de quartzo, caixa em aço inox com PVD preto, mostrador dourado e pulseira em couro legítimo. Resistente a 5 ATM, com vidro de safira antirreflexo.\n\nUma peça de precisão com presença discreta.",
                    "Aço inox PVD. Movimento suíço. Vidro safira. Couro legítimo.", "Evitar contato com água quente. Revisão a cada 2 anos.",
                    1899.90m, 2199.90m, "JLM-ACC-002", 11, "acessorios", "noite-dourada", 4.9m, 37,
                    true, false, true, "Único", "Preto:#101010,Dourado:#C9A45C", 20, 2),
                NewProduct("Boné Signature Gold", "bone-signature-gold", "bone-signature-gold",
                    "Boné estruturado em sarja com bordado dourado 3D.",
                    "Boné de seis gomos em sarja de algodão, aba curva e bordado JLM em dourado 3D. Fecho traseiro em metal com ajuste preciso.\n\nO detalhe streetwear que fecha qualquer composição.",
                    "100% algodão. Bordado 3D. Fecho metálico ajustável.", "Lavar à mão em água fria. Secar naturalmente.",
                    159.90m, 199.90m, "JLM-ACC-003", 45, "acessorios", "rua-e-presenca", 4.8m, 64,
                    false, false, true, "Único", "Preto:#101010,Grafite:#292929", 21, 3),
                NewProduct("Mochila Urban Premium", "mochila-urban-premium", "mochila-urban-premium",
                    "Nylon balístico, compartimento para notebook 15\" e detalhes em couro.",
                    "Mochila em nylon balístico resistente à água, com compartimento acolchoado para notebook de até 15 polegadas, bolso térmico e alças em couro legítimo. Fecho magnético oculto e base reforçada.\n\nPensada para o dia a dia urbano sem abrir mão do design.",
                    "Nylon balístico 1680D. Couro legítimo. Forro acolchoado.", "Limpar com pano úmido. Não submergir.",
                    459.90m, 549.90m, "JLM-ACC-004", 19, "acessorios", "essenciais-jlm", 4.8m, 31,
                    false, true, false, "Único", "Preto:#101010,Grafite:#292929", 22, 1),
                NewProduct("Cinto Couro Premium", "cinto-couro-premium", "cinto-couro-premium",
                    "Couro legítimo com fivela dourada escovada e ajuste fino.",
                    "Cinto em couro legítimo de 3,5cm, com fivela dourada escovada e sistema de ajuste fino. O acabamento das bordas é feito à mão, com cera natural.\n\nUm clássico que ganha personalidade com o tempo.",
                    "Couro legítimo. Fivela em latão banhado a ouro. 3,5cm.", "Limpar com pano seco. Evitar umidade.",
                    279.90m, null, "JLM-ACC-005", 34, "acessorios", "essenciais-jlm", 4.7m, 44,
                    false, false, false, "95,100,105,110", "Marrom:#4A3A2E,Preto:#101010", 23, 2),
                NewProduct("Carteira Slim Leather", "carteira-slim-leather", "carteira-slim-leather",
                    "Couro legítimo, perfil slim e proteção RFID.",
                    "Carteira slim em couro legítimo com costura manual, seis compartimentos para cartões, porta-notas e proteção RFID. Perfil fino que não deforma o bolso.\n\nMonograma JLM em relevo seco.",
                    "Couro legítimo. Proteção RFID. 11 x 8,5cm.", "Evitar contato com água. Hidratar o couro periodicamente.",
                    199.90m, 249.90m, "JLM-ACC-006", 41, "acessorios", "essenciais-jlm", 4.8m, 52,
                    false, false, true, "Único", "Marrom:#4A3A2E,Preto:#101010", 24, 3),
                NewProduct("Tênis Street Luxe", "tenis-street-luxe", "tenis-street-luxe",
                    "Couro premium, solado em borracha e detalhe dourado na entressola.",
                    "Tênis de couro premium com solado em borracha vulcanizada e detalhe dourado na entressola. Forro em couro, palmilha removível e cadarço em algodão encerado.\n\nO equilíbrio entre conforto e luxo casual.",
                    "Couro premium. Forro em couro. Solado de borracha.", "Limpar com pano úmido. Não lavar em máquina.",
                    799.90m, 949.90m, "JLM-ACC-007", 17, "acessorios", "rua-e-presenca", 4.9m, 46,
                    true, true, false, "38,39,40,41,42,43,44", "Off-white:#EAE3D6,Preto:#101010", 25, 1),
                NewProduct("Bolsa Transversal Urban", "bolsa-transversal-urban", "bolsa-transversal-urban",
                    "Compacta, em couro legítimo, com alça ajustável e fecho magnético.",
                    "Bolsa transversal em couro legítimo com alça ajustável, fecho magnético e dois compartimentos internos. Tamanho compacto para o essencial, com acabamento em costura manual.\n\nUma alternativa elegante à mochila.",
                    "Couro legítimo. Alça ajustável. Forro em algodão.", "Limpar com pano seco. Evitar exposição prolongada ao sol.",
                    389.90m, null, "JLM-ACC-008", 23, "acessorios", "noite-dourada", 4.8m, 22,
                    false, true, true, "Único", "Preto:#101010,Marrom:#4A3A2E", 26, 2)
            };

            db.Products.AddRange(products);

            var categoryIds = categories.ToDictionary(c => c.Key, c => c.Value.Id);
            var collectionIds = collections.ToDictionary(c => c.Key, c => c.Value.Id);
            var assignments = new (string Category, string Collection)[]
            {
                ("roupas", "rua-e-presenca"), ("roupas", "essenciais-jlm"), ("roupas", "rua-e-presenca"),
                ("roupas", "rua-e-presenca"), ("roupas", "essenciais-jlm"), ("roupas", "rua-e-presenca"),
                ("roupas", "noite-dourada"), ("roupas", "essenciais-jlm"), ("roupas", "edicao-laumir"),
                ("roupas", "edicao-laumir"), ("joias", "noite-dourada"), ("joias", "edicao-laumir"),
                ("joias", "noite-dourada"), ("joias", "edicao-laumir"), ("joias", "edicao-laumir"),
                ("joias", "noite-dourada"), ("joias", "noite-dourada"), ("joias", "rua-e-presenca"),
                ("acessorios", "edicao-laumir"), ("acessorios", "noite-dourada"), ("acessorios", "rua-e-presenca"),
                ("acessorios", "essenciais-jlm"), ("acessorios", "essenciais-jlm"), ("acessorios", "essenciais-jlm"),
                ("acessorios", "rua-e-presenca"), ("acessorios", "noite-dourada")
            };

            for (var i = 0; i < products.Count; i++)
            {
                products[i].CategoryId = categoryIds[assignments[i].Category];
                products[i].CollectionId = collectionIds[assignments[i].Collection];
            }

            await db.SaveChangesAsync();

            foreach (var product in products)
            {
                db.ProductImages.AddRange(
                    new ProductImage { ProductId = product.Id, ImageUrl = $"/images/products/{product.Slug}-detail.svg", AltText = $"{product.Name} — detalhe", SortOrder = 2 },
                    new ProductImage { ProductId = product.Id, ImageUrl = $"/images/products/{product.Slug}.svg", AltText = product.Name, SortOrder = 1 });
            }
            await db.SaveChangesAsync();
        }

        if (!db.Coupons.Any())
        {
            db.Coupons.AddRange(
                new Coupon
                {
                    Code = "BEMVINDO10",
                    Description = "10% de desconto na primeira compra",
                    DiscountType = DiscountType.Percentage,
                    Value = 10,
                    MinimumOrder = 199.90m,
                    MaxUses = 500,
                    StartsAt = DateTime.UtcNow.AddMonths(-2),
                    EndsAt = DateTime.UtcNow.AddMonths(4)
                },
                new Coupon
                {
                    Code = "PRESENCA50",
                    Description = "R$ 50 de desconto em compras acima de R$ 399",
                    DiscountType = DiscountType.Fixed,
                    Value = 50,
                    MinimumOrder = 399.90m,
                    MaxUses = 300,
                    StartsAt = DateTime.UtcNow.AddMonths(-1),
                    EndsAt = DateTime.UtcNow.AddMonths(3)
                },
                new Coupon
                {
                    Code = "NOITE15",
                    Description = "15% de desconto na linha Noite Dourada",
                    DiscountType = DiscountType.Percentage,
                    Value = 15,
                    MinimumOrder = 899.90m,
                    MaxUses = 100,
                    StartsAt = DateTime.UtcNow.AddMonths(-1),
                    EndsAt = DateTime.UtcNow.AddMonths(2)
                });
        }

        if (!db.ContactMessages.Any())
        {
            db.ContactMessages.AddRange(
                new ContactMessage
                {
                    Name = "Carlos Eduardo",
                    Email = "carlos.eduardo@email.com",
                    Phone = "(11) 97777-1122",
                    Subject = "Troca de tamanho",
                    Message = "Comprei o moletom Presença em G e gostaria de trocar por GG. Como procedo?",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new ContactMessage
                {
                    Name = "Marcos Vinícius",
                    Email = "marcos.v@email.com",
                    Subject = "Joia sob medida",
                    Message = "Gostaria de saber se é possível encomendar a corrente cubana com 60cm.",
                    IsRead = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                });
        }

        if (!db.NewsletterSubscribers.Any())
        {
            db.NewsletterSubscribers.AddRange(
                new NewsletterSubscriber { Email = "rafael.moreira@email.com", CreatedAt = DateTime.UtcNow.AddDays(-15) },
                new NewsletterSubscriber { Email = "contato.estilo@email.com", CreatedAt = DateTime.UtcNow.AddDays(-4) });
        }

        await db.SaveChangesAsync();

        if (!db.Orders.Any())
        {
            var customer = db.Users.First(u => u.Role == "Customer");
            var allProducts = db.Products.OrderBy(p => p.Id).ToList();

            var orders = new List<Order>
            {
                BuildOrder("JLM-1001", customer, allProducts[2], OrderStatus.Delivered, PaymentMethod.Pix, ShippingMethod.Standard, DateTime.UtcNow.AddDays(-22), "P,M"),
                BuildOrder("JLM-1002", customer, allProducts[10], OrderStatus.Shipped, PaymentMethod.CreditCard, ShippingMethod.Express, DateTime.UtcNow.AddDays(-9), "Único"),
                BuildOrder("JLM-1003", customer, allProducts[18], OrderStatus.Processing, PaymentMethod.CreditCard, ShippingMethod.Standard, DateTime.UtcNow.AddDays(-5), "Único"),
                BuildOrder("JLM-1004", customer, allProducts[0], OrderStatus.Paid, PaymentMethod.Pix, ShippingMethod.Standard, DateTime.UtcNow.AddDays(-2), "G"),
                BuildOrder("JLM-1005", customer, allProducts[19], OrderStatus.Pending, PaymentMethod.Boleto, ShippingMethod.Standard, DateTime.UtcNow.AddDays(-1), "Único")
            };

            // Add a second item to the first order for a richer order detail screen.
            var first = orders[0];
            var secondProduct = allProducts[0];
            first.Items.Add(new OrderItem
            {
                ProductId = secondProduct.Id,
                ProductName = secondProduct.Name,
                ProductImageUrl = secondProduct.ImageUrl,
                Size = "M",
                Color = "Preto",
                Quantity = 1,
                UnitPrice = secondProduct.Price,
                LineTotal = secondProduct.Price
            });
            first.Subtotal += secondProduct.Price;
            first.Total = first.Subtotal + first.ShippingCost - first.Discount;

            db.Orders.AddRange(orders);
            await db.SaveChangesAsync();
        }
    }

    private static Product NewProduct(
        string name, string slug, string imageSlug, string shortDescription, string description,
        string composition, string care, decimal price, decimal? comparePrice, string sku, int stock,
        string categorySlug, string collectionSlug, decimal rating, int reviewCount,
        bool featured, bool isNew, bool bestSeller, string sizes, string colors, int sort, int collectionOrder)
    {
        return new Product
        {
            Name = name,
            Slug = slug,
            ShortDescription = shortDescription,
            Description = description,
            Composition = composition,
            CareInstructions = care,
            Price = price,
            ComparePrice = comparePrice,
            Sku = sku,
            Stock = stock,
            Rating = rating,
            ReviewCount = reviewCount,
            IsFeatured = featured,
            IsNewArrival = isNew,
            IsBestSeller = bestSeller,
            Sizes = sizes,
            Colors = colors,
            ImageUrl = $"/images/products/{imageSlug}.svg",
            Category = null!,
            CategoryId = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-sort * 3)
        };
    }

    private static Order BuildOrder(string number, User customer, Product product, OrderStatus status,
        PaymentMethod payment, ShippingMethod shipping, DateTime createdAt, string size)
    {
        var shippingCost = product.Price >= 500 ? 0 : shipping == ShippingMethod.Express ? 49.90m : 29.90m;
        var subtotal = product.Price;

        return new Order
        {
            OrderNumber = number,
            UserId = customer.Id,
            CustomerName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone ?? string.Empty,
            Cpf = customer.Cpf,
            ShippingZipCode = customer.ZipCode ?? "05422-001",
            ShippingAddress = customer.Address ?? "Rua dos Pinheiros",
            ShippingNumber = customer.AddressNumber ?? "842",
            ShippingComplement = customer.Complement,
            ShippingNeighborhood = customer.Neighborhood ?? "Pinheiros",
            ShippingCity = customer.City ?? "São Paulo",
            ShippingState = customer.State ?? "SP",
            Subtotal = subtotal,
            ShippingCost = shippingCost,
            Discount = 0,
            Total = subtotal + shippingCost,
            PaymentMethod = payment,
            ShippingMethod = shipping,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = createdAt.AddHours(4),
            Items = new List<OrderItem>
            {
                new()
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImageUrl = product.ImageUrl,
                    Size = size,
                    Color = product.ColorList.FirstOrDefault().Name ?? "Preto",
                    Quantity = 1,
                    UnitPrice = product.Price,
                    LineTotal = product.Price
                }
            }
        };
    }
}
