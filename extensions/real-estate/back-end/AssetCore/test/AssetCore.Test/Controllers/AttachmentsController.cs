using AssetCore.TwinCreatorAsset.Dto;
using AssetCoreTwinCreator.Domain;
using AssetCoreTwinCreator.Domain.Models;
using AssetCoreTwinCreator.MappingId;
using AssetCoreTwinCreator.MappingId.Extensions;
using AssetCoreTwinCreator.MappingId.Models;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;
using Moq;
using AssetFile = AssetCoreTwinCreator.Features.Asset.Attachments.Models.File;

using Willow.Common;

using AssetCoreTwinCreator.Features.Asset.Attachments;

using Microsoft.AspNetCore.Mvc;

namespace AssetCore.Test.Controllers
{
    public class AttachmentsControllerTests 
    {
        private readonly Mock<IAttachmentsService> _attachmentSvc = new Mock<IAttachmentsService>();
        private readonly Mock<IBlobStore>          _blobSvc       = new Mock<IBlobStore>();
        private readonly Mock<IMappingService>     _mapSvc        = new Mock<IMappingService>();

        public AttachmentsControllerTests()
        {
            _attachmentSvc.Setup( s=> s.GetFile(5000) ).ReturnsAsync( new AssetFile { BlobName = "bob" } );
            _blobSvc.Setup( api=> api.Get("bob", It.IsAny<Stream>()) ).Callback((string id, Stream content)=> content.Write(UTF8Encoding.Default.GetBytes("bobs your uncle"), 0, UTF8Encoding.Default.GetByteCount("bobs your uncle")));
        }

        [Fact]
        public async Task AttachmentsController_Get()
        {
            var siteId     = Guid.NewGuid();
            var controller = new AttachmentsController(_attachmentSvc.Object, _blobSvc.Object, _mapSvc.Object);

            var result = (await controller.GetFileContent(siteId, 5000.ToFileGuid())) as FileStreamResult;

            Assert.NotNull(result);
            Assert.Equal("bobs your uncle", UTF8Encoding.Default.GetString((result.FileStream as MemoryStream).ToArray()));
        }
    }
}
