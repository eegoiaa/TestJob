using FluentValidation;
using TestJob.Api.Models;

namespace TestJob.Api.Validation;

public class ProcessRequestValidator : AbstractValidator<ProcessRequest>
{
    public ProcessRequestValidator()
    {
        RuleFor(x => x.Selector)
            .NotEmpty().WithMessage("Selector is required and cannot be empty.");

        RuleFor(x => x.Attribute)
            .NotEmpty().WithMessage("Attribute is required and cannot be empty.");

        RuleFor(x => x.UrlB64)
            .NotEmpty().WithMessage("url_b64 is required.");

        RuleFor(x => x.EncryptedTextBytesB64)
            .NotEmpty().WithMessage("encrypted_text_bytes_b64 is required.");

        RuleFor(x => x.KeyBytesB64)
            .NotEmpty().WithMessage("key_bytes_b64 is required.");

        RuleFor(x => x.PageB64)
            .NotEmpty().WithMessage("page_b64 is required.");
    }
}
