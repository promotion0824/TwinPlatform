namespace Connector.Nunit.Tests.TestData
{
    using System;

    public class Constants
    {
        public static Guid SiteIdDefault = Guid.Parse("6a8cb6ef-f23b-4608-a08b-0b779fd616cb");

        public static Guid ClientIdDefault = Guid.Parse("8b242df4-e0fa-4c15-99b4-66597a8d56ae");

        public static Guid LocationIdSynthetic = Guid.Parse("0ec4185b-b9bd-4b7a-a0c0-11c55a2793b1");

        public static Guid LocationIdPrimary = Guid.Parse("4586e570-d14d-43f0-9d69-bf7a0c7ad54a");

        public static Guid LocationIdSecondary = Guid.Parse("b4db8740-6f07-439c-a419-a4aa1cbf5e12");

        public static Guid PointId1 = Guid.Parse("1c58f9d2-6d32-4b2f-9b6a-dd5255714bf1");
        public static Guid PointId2 = Guid.Parse("77e1f782-6588-4276-a5c2-fba9d99396b8");
        public static Guid PointId3 = Guid.Parse("5c17e4cb-0af3-4b1e-926c-6091b8200f6a");
        public static Guid PointId4 = Guid.Parse("8ab1be3d-b67c-4cdc-bbcd-7ff95c20a571");
        public static Guid PointId5ForValidation = Guid.Parse("d3ee30ee-2513-4515-8c18-8e0f462366a4");
        public static Guid PointId6ForValidation = Guid.Parse("fbc874c5-9945-4238-a427-2c6edacd52c2");
        public static Guid PointId7ForValidation = Guid.Parse("64b07867-a2f2-487a-84ff-0450b2cf965f");

        public static string PointExternalId1 = "4779737b-2816-4327-a52b-8b458fcca0bc";
        public static string PointExternalId2 = "f8dbdb69-ee2c-4180-91df-b39872aad69d";
        public static string PointExternalId3 = "b195b0c4-5905-4a50-ba4a-6e938a8b4b40";
        public static string PointExternalId4 = "a4ab2b6e-f771-4eea-85ca-a77120cfc581";
        public static string PointExternalId5 = "89eb994f-93d3-44d3-a125-163211d54241";
        public static string PointExternalId6 = "6f353b5a-598a-4743-b42a-94d0a45546f8";
        public static string PointExternalId7 = "efce3c66-b598-40cd-b363-d231619d57b0";

        public static Guid DeviceId1 = Guid.Parse("ca32df47-d84c-4aef-b18d-1b2fa5cd6868");
        public static Guid DeviceId2 = Guid.Parse("3eb68bff-d530-4da8-b537-db3d48fde21c");
        public static Guid DeviceId3 = Guid.Parse("6cad3012-f580-40af-9d26-b438e4466cd6");
        public static Guid DeviceIdForValidation = Guid.Parse("2f54b331-dc4c-4be9-abb3-8c1f9cd82ec5");
        public static Guid DeviceIdForValidationNotFirst = Guid.Parse("7d041381-a5c1-495c-8482-b6c3f269588b");

        public static string DeviceExternalId1 = "e13e5144-00fa-4afe-b40f-f7b3267912bb";
        public static string DeviceExternalId2 = "e49b599f-f0f9-4379-b3a5-051611738be9";
        public static string DeviceExternalId3 = "bfee430a-92fe-4f94-987f-24d3b4dd5f5f";
        public static string DeviceExternalIdForValidation = "d280d59d-189b-4ba1-9f7f-4defe390de60";
        public static string DeviceExternalIdForValidationNotFirst = "ec582af4-2d31-478e-86a3-23fc4c6d740d";

        public static Guid TagIdToAddToPoint1 = Guid.Parse("033693dd-8f24-43cf-90b1-ff344a0247ff");
        public static Guid TagIdToAddToPoint2 = Guid.Parse("c4f83a4a-9c48-4f40-86bd-656bc020e682");
        public static Guid TagIdToAddToEquipment1 = Guid.Parse("e5f7eb09-7848-48a6-a44b-980c60281950");
        public static Guid TagIdToAddToEquipment2 = Guid.Parse("88a71253-8a0c-428a-991b-2d4f322af7d2");

        public static Guid EquipmentId1 = Guid.Parse("59465e4c-c494-4f1a-a041-0389b9003f60");
        public static Guid EquipmentId2 = Guid.Parse("e3241d49-f9fb-409a-8be9-d535414fb1f5");
        public static Guid EquipmentId3 = Guid.Parse("10f5ed7d-1bdf-494f-a327-02f7f50fb09d");
        public static Guid EquipmentId4 = Guid.Parse("33d691c8-20da-4eb6-839c-9a238a2de751");
        public static Guid EquipmentId5 = Guid.Parse("d448802f-c768-4b2a-a2ca-210c0ef4832f");
        public static Guid EquipmentId6 = Guid.Parse("b0a24235-f563-4b90-b40d-7cc6691b9909");
        public static Guid EquipmentId7 = Guid.Parse("742a7fd2-4ae8-4a58-96c5-83498dd72b93");
        public static Guid EquipmentId8 = Guid.Parse("673feec5-9450-48e6-a95f-35bee62666b4");
        public static Guid EquipmentId9 = Guid.Parse("b309f912-8622-41fe-b6e1-499c8a68c6ff");
        public static Guid EquipmentId10ForValidation = Guid.Parse("717a2ae5-7835-4d4b-a4fd-56616b4ddc10");

        public static Guid SchemaId1 = Guid.Parse("b4d9c647-5e5f-4ecb-ad2c-00e14be967f0");
        public static Guid SchemaId2 = Guid.Parse("fb2c7a16-d914-4aca-abfe-fb67548f3766");

        public static Guid ConnectorTypeId1 = Guid.Parse("5113b3bf-0de6-4a56-818e-7828f938b6fa");
        public static Guid ConnectorTypeId2 = Guid.Parse("276be07d-bd73-43a2-8b38-95888672f196");
        public static Guid ConnectorTypeId3 = Guid.Parse("93c894fa-42e3-4566-978c-61ff9d905b2a");
        public static Guid ConnectorTypeId4 = Guid.Parse("856a76da-e8e0-4f65-ae2b-f14a087725a2");
        public static Guid ConnectorTypeId5 = Guid.Parse("49253e6b-fed3-42b7-8daf-8b2678752376");

        public static Guid ConnectorId1 = Guid.Parse("360d2af2-446b-4b1f-bb6f-20af553ea289");
        public static Guid ConnectorId2 = Guid.Parse("116bdce3-4ad1-48ce-9585-4adefa894efa");
        public static Guid ConnectorId3ToDelete = Guid.Parse("a11ee966-6e2b-4a7c-b300-bcaa04a5fb94");
        public static Guid ConnectorId4ToDelete = Guid.Parse("1dd7684e-562f-4afa-904f-7ef4903c1b85");
        public static Guid ConnectorId5 = Guid.Parse("2207dd6c-228f-4cad-86fb-bac7af5800b7");
        public static Guid ConnectorId6 = Guid.Parse("f7923423-bc96-4fab-ad44-1335c0af3eb5");
        public static Guid ConnectorId7 = Guid.Parse("332d68a8-8c32-4dac-9950-7198aa12a7be");
        public static Guid ConnectorIdForValidation = Guid.Parse("563c90e1-2395-4cfb-979b-523ca822db69");
        public static Guid ConnectorIdForValidationNotFirst = Guid.Parse("ae49a784-beb2-464f-b830-c90901db0456");

        public static string EquipmentCategory1 = "Category 1";
        public static string EquipmentCategory2 = "Category 2";
        public static string EquipmentCategory3 = "Category 3";

        public static string TestLogsSource = "TestSource";
        public static string SenderLogsSource = "Sender";

        public static Guid TagCategoryId1 = Guid.Parse("58b210c3-d8a4-41ac-bda7-533e07d420a2");
        public static Guid TagCategoryId2 = Guid.Parse("408dba2c-663c-422e-8239-e4e8ec4d6dba");
        public static Guid TagCategoryId3ToDelete = Guid.Parse("4fb11805-6b2b-4675-9d2d-8b73ac201d67");
        public static Guid TagCategoryId4ToRemoveTags = Guid.Parse("ad5ed19e-50b9-4904-a9cf-5b4267560d81");
        public static Guid TagCategoryId5ToAddTags = Guid.Parse("c0c0abdc-d3de-4117-88b7-ba500d8e5ff8");

        public static Guid TagForCategoryId1 = Guid.Parse("84d22011-4fae-46e6-9166-901a9bcfa861");
        public static Guid TagForCategoryId2 = Guid.Parse("b0561d0a-0f92-489a-bb89-f33f7c51f7af");
        public static Guid TagForCategoryId3 = Guid.Parse("72066239-92f5-4666-8e34-2d6bff1b84de");

        public static Guid ScanRequestId1 = Guid.Parse("33c19f77-5471-40f1-a7ad-1ec1d8e387d3");
        public static Guid ScanRequestId2 = Guid.Parse("93970785-f619-4a53-9667-e61537b4307c");
        public static Guid ScanRequestId3Deleting = Guid.Parse("65b711c4-899a-4dfd-9a3c-d1ea5bac0f06");
        public static Guid ScanRequestId4 = Guid.Parse("3bf747c3-fa18-4d80-ab31-5620b34c47e5");

        public static Guid TagId1 = Guid.Parse("8d19ca11-cb24-4bfe-95c0-83eb1e572c3e");
        public static Guid TagId2 = Guid.Parse("c3a0cdaf-e816-42b5-8fe9-e48a419d700d");
        public static Guid TagId3 = Guid.Parse("0b39895f-1ab8-4a41-aa9c-f0558b2228a0");
        public static Guid TagId4 = Guid.Parse("3dc6c36c-62a7-4b5f-a01a-a53e75f837b7");
        public static Guid TagId5ToUpdate = Guid.Parse("7790a6f3-09c6-47e5-b35a-55f968fc5814");
        public static Guid TagId6 = Guid.Parse("12c6c36c-62a7-4b5f-a01a-a53e75f837b7");
        public static Guid TagId7 = Guid.Parse("23c6c36c-62a7-4b5f-a01a-a53e75f837b7");
        public static Guid TagId8 = Guid.Parse("34c6c36c-62a7-4b5f-a01a-a53e75f837b7");

        public static Guid TagId1ForFeature = Guid.Parse("750e594d-41e3-4fe5-8484-5fa791a0cf34");
        public static Guid TagId2ForFeature = Guid.Parse("06a66d9e-39cd-4f50-9cd7-cd0e55ffebf3");

        public static Guid FloorId1 = Guid.Parse("e4666e0f-ce44-4479-8049-511f74017913");

        public static string TagFeature1 = "TagFeature1";

        public static Guid GatewayId1 = Guid.Parse("7CCE88DA-6649-4637-872C-7F0481FBAF1C");
        public static Guid GatewayId2 = Guid.Parse("8979ECBD-C95B-42DA-8C64-99DDC28C5426");
        public static Guid GatewayId3 = Guid.Parse("AE8C13B2-6E4E-4B2E-99A1-6B84787A5094");
        public static Guid GatewayId4 = Guid.Parse("05390110-5599-4C67-8A73-65EFCF595D5C");
    }
}
