using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureManager : MonoBehaviour
{
    public Texture2DArray VoxelTextures;

    private void Awake()
    {
        var textures = Resources.LoadAll<Texture2D>("Textures");
        if (textures.Length > 0)
        {
            VoxelTextures = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, textures[0].format, false);
            for (int i = 0; i < textures.Length; i++)
            {
                Graphics.CopyTexture(textures[i], 0, 0, VoxelTextures, i, 0);
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
