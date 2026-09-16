using System.Security.Claims;
using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? courseId = null)
    {
        var userId = GetUserId();
        return userId is null ? Unauthorized() : Ok(await _commentService.GetAllAsync(userId, IsAdministrator(), courseId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var comment = await _commentService.GetByIdAsync(id, userId, IsAdministrator());
        return comment is null ? NotFound() : Ok(comment);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCommentRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var comment = await _commentService.CreateAsync(userId, request);
        return comment is null
            ? Conflict(new { message = Messages.Comments.CreateConflict })
            : CreatedAtAction(nameof(GetById), new { id = comment.Id }, comment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateCommentRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var comment = await _commentService.UpdateAsync(id, userId, IsAdministrator(), request);
        return comment is null ? NotFound() : Ok(comment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        return await _commentService.DeleteAsync(id, userId, IsAdministrator()) ? NoContent() : NotFound();
    }

    [HttpPost("{id}/reactions")]
    public async Task<IActionResult> AddReaction(string id, CommentReactionRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var comment = await _commentService.AddReactionAsync(id, userId, request);
        return comment is null ? NotFound() : Ok(comment);
    }

    [HttpDelete("{id}/reactions")]
    public async Task<IActionResult> RemoveReaction(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var comment = await _commentService.RemoveReactionAsync(id, userId);
        return comment is null ? NotFound() : Ok(comment);
    }

    [HttpPost("{id}/archive")]
    public async Task<IActionResult> Archive(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var comment = await _commentService.ArchiveAsync(id, userId);
        return comment is null ? NotFound() : Ok(comment);
    }

    [HttpDelete("{id}/archive")]
    public async Task<IActionResult> Unarchive(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var comment = await _commentService.UnarchiveAsync(id, userId);
        return comment is null ? NotFound() : Ok(comment);
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private bool IsAdministrator() => User.IsInRole("Administrador");
}