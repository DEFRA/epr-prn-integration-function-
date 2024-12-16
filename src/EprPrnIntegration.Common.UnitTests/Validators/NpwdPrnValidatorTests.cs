﻿using AutoFixture;
using EprPrnIntegration.Common.Models;
using EprPrnIntegration.Common.Validators;
using FluentValidation.TestHelper;

namespace EprPrnIntegration.Common.UnitTests.Validators
{
    public class NpwdPrnValidatorTests
    {
        private NpwdPrnValidator _sut = null!;
        private Fixture _fixture = new Fixture();
        public NpwdPrnValidatorTests()
        {
            _sut = new NpwdPrnValidator();
        }

        // Accreditation No
        [Fact]
        public void AccreditationNo_Should_Not_Have_Error_When_Is_Valid()
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            var result = _sut.TestValidate(npwdPrn);
            result.ShouldNotHaveValidationErrorFor(x => x.AccreditationNo);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AccreditationNo_Should_Have_Error_When_Is_NulllOrEmpty(string? npwdAccreditationNo)
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            npwdPrn.AccreditationNo = npwdAccreditationNo;
            var result = _sut.TestValidate(npwdPrn);
            result.ShouldHaveValidationErrorFor(x => x.AccreditationNo);
        }

        // EvidenceNo, equates to PRN Number
        [Fact]
        public void EvidenceNo_Should_Not_Have_Error_When_Is_Valid()
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            var result = _sut.TestValidate(npwdPrn);
            result.ShouldNotHaveValidationErrorFor(x => x.EvidenceNo);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void EvidenceNo_Should_Have_Error_When_Is_NulllOrEmpty(string? npwdEvidenceNo)
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            npwdPrn.EvidenceNo = npwdEvidenceNo;
            var result = _sut.TestValidate(npwdPrn);
            result.ShouldHaveValidationErrorFor(x => x.EvidenceNo);
        }

        // IssuedToEPRId, equates to organisation id
        [Fact]
        public void IssuedEPRId_Should_Not_Have_Error_When_Is_Valid()
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            var orgId = Guid.NewGuid();
            npwdPrn.IssuedToEPRId = orgId.ToString();
            npwdPrn.ValidOrganisationIds = new List<Guid> { orgId };
            var result = _sut.TestValidate(npwdPrn);
            result.ShouldNotHaveValidationErrorFor(x => x.IssuedToEPRId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("OrganisationId")]
        [InlineData("05be0802-19ac-4c80-b99d-6452577bf93d")]
        public void IssuedEPRId_Should_Have_Error_When_Is_NulllOrEmpty_Or_Not_Guid_Or_Invalid_Guid(string? npwdEprId)
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            npwdPrn.IssuedToEPRId = npwdEprId;
            var result = _sut.TestValidate(npwdPrn);
            result.ShouldHaveValidationErrorFor(x => x.IssuedToEPRId);
        }

        // Tonnage
        [Fact]
        public void Tonnage_Should_Not_Have_Error_When_Is_Valid()
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            var result = _sut.TestValidate(npwdPrn);
            result.ShouldNotHaveValidationErrorFor(x => x.EvidenceTonnes);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Tonnage_Should_Have_Error_When_Is_Not_Valid(int npwdTonnage)
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            npwdPrn.EvidenceTonnes = npwdTonnage;
            var result = _sut.TestValidate(npwdPrn);
            result.ShouldHaveValidationErrorFor(x => x.EvidenceTonnes);
        }

        // Material e.g. paper
        [Theory]
        [InlineData("Aluminium")]
        [InlineData("Glass")]
        [InlineData("Paper")]
        [InlineData("Plastic")]
        [InlineData("Steel")]
        [InlineData("Wood")]
        [InlineData("aluminium")]
        public void EvidenceMaterial_Should_Not_Have_Error_When_Is_Valid(string? npwdMaterial)
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            npwdPrn.EvidenceMaterial = npwdMaterial;
            var result = _sut.TestValidate(npwdPrn);
            result.ShouldNotHaveValidationErrorFor(x => x.EvidenceMaterial);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Zinc")]
        public void EvidenceMaterial_Should_Have_Error_When_Is_NulllOrEmpty_Or_InvalidMaterial(string? npwdMaterial)
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            npwdPrn.EvidenceMaterial = npwdMaterial;
            var result = _sut.TestValidate(npwdPrn);
            result.ShouldHaveValidationErrorFor(x => x.EvidenceMaterial);
        }

        // Accreditation Year
        private const int MinAccreditationYear = 2025;
        [Fact]
        public void AccrediationYear_Should_Not_Have_Error_When_Is_Valid()
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            int maxYear =  DateTime.UtcNow.Year + 1;
            for (int year = MinAccreditationYear; year < maxYear; year++)
            {
                npwdPrn.AccreditationYear = year;
                var result = _sut.TestValidate(npwdPrn);
                result.ShouldNotHaveValidationErrorFor(x => x.AccreditationYear);
            }
        }

        [Fact]
        public void AccreditationYear_Should_Have_Error_When_Is_Out_Of_Bounds()
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            npwdPrn.AccreditationYear = MinAccreditationYear - 1;
            var result = _sut.TestValidate(npwdPrn);
            result.ShouldHaveValidationErrorFor(x => x.AccreditationYear);

            npwdPrn.AccreditationYear = MinAccreditationYear + 2;
            result = _sut.TestValidate(npwdPrn);
            result.ShouldHaveValidationErrorFor(x => x.AccreditationYear);
        }

        // Cancelled Date
        [Fact]
        public void CancelledDate_Should_Not_Have_Error_When_Is_Valid()
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            npwdPrn.CancelledDate = null;
            var result = _sut.TestValidate(npwdPrn);
            result.ShouldNotHaveValidationErrorFor(x => x.CancelledDate);

            npwdPrn.EvidenceStatusCode = "EV-CANCEL";
            npwdPrn.CancelledDate = DateTime.UtcNow;
            result = _sut.TestValidate(npwdPrn);
            result.ShouldNotHaveValidationErrorFor(x => x.CancelledDate);
        }

        [Fact]
        public void CancelledDate_Should_Have_Error_When_Is_Null_And_Status_Is_Cancelled()
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            npwdPrn.EvidenceStatusCode = "EV-CANCEL";
            npwdPrn.CancelledDate = null;

            var result = _sut.TestValidate(npwdPrn);
            result.ShouldHaveValidationErrorFor(x => x.CancelledDate)
                .WithErrorMessage("Cancellation date must not be null when PRN has status of EV-CANCEL");

        }

        [Fact]
        public void CancelledDate_Should_Have_Error_When_Is_Not_Null_And_Status_Is_Not_Cancelled()
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            npwdPrn.EvidenceStatusCode = "EV-AWACCEP";
            npwdPrn.CancelledDate = DateTime.UtcNow;

            var result = _sut.TestValidate(npwdPrn);
            result.ShouldHaveValidationErrorFor(x => x.CancelledDate)
                 .WithErrorMessage("Cancellation date must be null when PRN is not cancelled");
        }

        // Issue Date
        [Fact]
        public void IssueDate_Should_Not_Have_Error_When_Is_Valid()
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            npwdPrn.IssueDate = DateTime.UtcNow;
            var result = _sut.TestValidate(npwdPrn);
            result.ShouldNotHaveValidationErrorFor(x => x.IssueDate);
        }

        [Fact]
        public void IssueDate_Should_Have_Error_When_Is_Nulll()
        {
            var npwdPrn = _fixture.Create<NpwdPrn>();
            npwdPrn.IssueDate = null;
            var result = _sut.TestValidate(npwdPrn);
            result.ShouldHaveValidationErrorFor(x => x.IssueDate);
        }

    }
}
