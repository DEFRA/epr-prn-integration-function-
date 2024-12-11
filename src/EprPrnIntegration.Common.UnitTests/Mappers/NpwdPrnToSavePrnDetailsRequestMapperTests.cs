﻿using System;
using EprPrnIntegration.Common.Constants;
using EprPrnIntegration.Common.Models;
using EprPrnIntegration.Common.Mappers;
using Xunit;

namespace EprPrnIntegration.Tests.Mappers
{
    public class NpwdPrnToSavePrnDetailsRequestMapperTests
    {
        // Test for the Map method
        [Fact]
        public void Map_ValidNpwdPrn_ReturnsMappedSavePrnDetailsRequest()
        {
            // Arrange
            var npwdPrn = new NpwdPrn
            {
                AccreditationNo = "12345",
                AccreditationYear = 2024,
                CancelledDate = new DateTime(2024, 12, 11),
                DecemberWaste = true,
                EvidenceMaterial = "Plastic",
                EvidenceNo = "EA123456",
                EvidenceStatusCode = "Active",
                EvidenceTonnes = 200,
                IssueDate = new DateTime(2024, 01, 01),
                IssuedByNPWDCode = "6B29FC40-CA47-1067-B31D-00DD010662DA",
                IssuedByOrgName = "Exporter Ltd.",
                IssuedToNPWDCode = "6B29FC40-CA47-1067-B31D-00DD010662DA",
                IssuedToOrgName = "Reprocessor Ltd.",
                IssuedToEPRId = "6b29fc40-ca47-1067-b31d-00dd010662da",
                IssuerNotes = "No Notes",
                IssuerRef = "IssuerRef123",
                MaterialOperationCode = "6b29fc40-ca47-1067-b31d-00dd010662da",
                ModifiedOn = new DateTime(2024, 11, 01),
                ObligationYear = 2024,
                PRNSignatory = "John Doe",
                PRNSignatoryPosition = "Manager",
                ProducerAgency = "AgencyA",
                RecoveryProcessCode = "R123",
                ReprocessorAgency = "AgencyB",
                StatusDate = new DateTime(2024, 11, 10)
            };

            // Act
            var result = NpwdPrnToSavePrnDetailsRequestMapper.Map(npwdPrn);
            var temp = Guid.Parse(npwdPrn.IssuedToNPWDCode);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(npwdPrn.AccreditationNo, result.AccreditationNo);
            Assert.Equal(npwdPrn.AccreditationYear.ToString(), result.AccreditationYear);
            Assert.Equal(npwdPrn.CancelledDate, result.CancelledDate);
            Assert.Equal(npwdPrn.DecemberWaste, result.DecemberWaste);
            Assert.Equal(npwdPrn.EvidenceMaterial, result.EvidenceMaterial);
            Assert.Equal(npwdPrn.EvidenceNo, result.EvidenceNo);
            Assert.Equal(NpwdStatusToPrnStatusMapper.Map(npwdPrn.EvidenceStatusCode!), result.EvidenceStatusCode);
            Assert.Equal(npwdPrn.EvidenceTonnes, result.EvidenceTonnes);
            Assert.Equal(npwdPrn.IssueDate, result.IssueDate);
            Assert.Equal(Guid.Parse(npwdPrn.IssuedByNPWDCode), result.IssuedByNPWDCode);
            Assert.Equal(npwdPrn.IssuedByOrgName, result.IssuedByOrgName);
            Assert.Equal(Guid.Parse(npwdPrn.IssuedToNPWDCode), result.IssuedToNPWDCode);
            Assert.Equal(npwdPrn.IssuedToOrgName, result.IssuedToOrgName);
            Assert.Equal(Guid.Parse(npwdPrn.IssuedToEPRId), result.IssuedToEPRId);
            Assert.Equal(npwdPrn.IssuerNotes, result.IssuerNotes);
            Assert.Equal(npwdPrn.IssuerRef, result.IssuerRef);
            Assert.Equal(Guid.Parse(npwdPrn.MaterialOperationCode), result.MaterialOperationCode);
            Assert.Equal(npwdPrn.ModifiedOn, result.ModifiedOn);
            Assert.Equal(npwdPrn.ObligationYear.ToString(), result.ObligationYear);
            Assert.Equal(npwdPrn.PRNSignatory, result.PrnSignatory);
            Assert.Equal(npwdPrn.PRNSignatoryPosition, result.PrnSignatoryPosition);
            Assert.Equal(npwdPrn.ProducerAgency, result.ProducerAgency);
            Assert.Equal(npwdPrn.RecoveryProcessCode, result.RecoveryProcessCode);
            Assert.Equal(npwdPrn.ReprocessorAgency, result.ReprocessorAgency);
            Assert.Equal(npwdPrn.StatusDate, result.StatusDate);
            Assert.Equal("IntegrationFA", result.CreatedByUser);
        }

        // Test for the IsExport method when evidenceNo starts with "EA"
        [Fact]
        public void IsExport_EvidenceNoStartsWithEa_ReturnsTrue()
        {
            // Arrange
            var evidenceNo = "EX123456";

            // Act
            var result = NpwdPrnToSavePrnDetailsRequestMapper.IsExport(evidenceNo);

            // Assert
            Assert.True(result);
        }

        // Test for the IsExport method when evidenceNo starts with "SEPA"
        [Fact]
        public void IsExport_EvidenceNoStartsWithSepa_ReturnsTrue()
        {
            // Arrange
            var evidenceNo = "SXPA123456";

            // Act
            var result = NpwdPrnToSavePrnDetailsRequestMapper.IsExport(evidenceNo);

            // Assert
            Assert.True(result);
        }

        // Test for the IsExport method when evidenceNo does not start with "EA" or "SEPA"
        [Fact]
        public void IsExport_EvidenceNoDoesNotStartWithEaOrSepa_ReturnsFalse()
        {
            // Arrange
            var evidenceNo = "XYZ123456";

            // Act
            var result = NpwdPrnToSavePrnDetailsRequestMapper.IsExport(evidenceNo);

            // Assert
            Assert.False(result);
        }

        // Test for the IsExport method when evidenceNo is empty
        [Fact]
        public void IsExport_EmptyEvidenceNo_ReturnsFalse()
        {
            // Arrange
            var evidenceNo = "";

            // Act
            var result = NpwdPrnToSavePrnDetailsRequestMapper.IsExport(evidenceNo);

            // Assert
            Assert.False(result);
        }

        // Test for the IsExport method when evidenceNo is null
        [Fact]
        public void IsExport_NullEvidenceNo_ReturnsFalse()
        {
            // Arrange
            string evidenceNo = null;

            // Act
            var result = NpwdPrnToSavePrnDetailsRequestMapper.IsExport(evidenceNo);

            // Assert
            Assert.False(result);
        }
    }
}
