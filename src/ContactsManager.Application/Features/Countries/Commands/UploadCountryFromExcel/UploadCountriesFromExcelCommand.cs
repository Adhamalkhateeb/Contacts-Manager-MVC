using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Countries.Commands.UploadCountryFromExcel;

public record UploadCountriesFromExcelCommand(Stream FileStream, string FileName, long FileSize)
    : IRequest<Result<UploadCountriesFromExcelResult>>;
