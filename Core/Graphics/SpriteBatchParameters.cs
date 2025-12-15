namespace HeavenlyArsenal.Core.Graphics;

public struct SpriteBatchParameters
(
    SpriteSortMode spriteSortMode,
    BlendState blendState,
    SamplerState samplerState,
    DepthStencilState depthStencilState,
    RasterizerState rasterizerState,
    Effect effect,
    Matrix transformMatrix
)
{
    /// <summary>
    ///     Gets or sets the <see cref="SpriteSortMode" /> of the captured <see cref="SpriteBatch" />
    ///     parameters.
    /// </summary>
    public SpriteSortMode SpriteSortMode { readonly get; set; } = spriteSortMode;

    /// <summary>
    ///     Gets or sets the <see cref="BlendState" /> of the captured <see cref="SpriteBatch" />
    ///     parameters.
    /// </summary>
    public BlendState BlendState { readonly get; set; } = blendState;

    /// <summary>
    ///     Gets or sets the <see cref="SamplerState" /> of the captured <see cref="SpriteBatch" />
    ///     parameters.
    /// </summary>
    public SamplerState SamplerState { readonly get; set; } = samplerState;

    /// <summary>
    ///     Gets or sets the <see cref="DepthStencilState" /> of the captured <see cref="SpriteBatch" />
    ///     parameters.
    /// </summary>
    public DepthStencilState DepthStencilState { readonly get; set; } = depthStencilState;

    /// <summary>
    ///     Gets or sets the <see cref="RasterizerState" /> of the captured <see cref="SpriteBatch" />
    ///     parameters.
    /// </summary>
    public RasterizerState RasterizerState { readonly get; set; } = rasterizerState;

    /// <summary>
    ///     Gets or sets the <see cref="Effect" /> of the captured <see cref="SpriteBatch" /> parameters.
    /// </summary>
    public Effect Effect { readonly get; set; } = effect;

    /// <summary>
    ///     Gets or sets the transform <see cref="Matrix" /> of the captured <see cref="SpriteBatch" />
    ///     parameters.
    /// </summary>
    public Matrix TransformMatrix { readonly get; set; } = transformMatrix;
}