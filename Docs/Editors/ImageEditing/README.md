{% include nav.html %}

# Image Editor User Guide

The SkyCMS Image Editor provides a powerful, browser-based tool for editing images directly within the CMS. Built on the Filerobot Image Editor, it offers professional-grade image manipulation capabilities without requiring external software.

## Table of Contents

- [Accessing the Image Editor](#accessing-the-image-editor)
- [Editor Interface Overview](#editor-interface-overview)
- [Editing Tools](#editing-tools)
  - [Adjust](#adjust)
  - [Annotate](#annotate)
  - [Filters](#filters)
  - [Finetune](#finetune)
  - [Resize](#resize)
  - [Watermark](#watermark)
- [Saving Your Work](#saving-your-work)
- [Supported Image Formats](#supported-image-formats)
- [Best Practices](#best-practices)

## Accessing the Image Editor

1. Navigate to the **File Manager** in your SkyCMS dashboard
2. Browse to the location of the image you want to edit
3. Select an image file (must be one of the supported formats)
4. Click the **Edit** button or right-click and select **Edit Image**

The image editor will open in a new view with your image loaded and ready for editing.

## Editor Interface Overview

The image editor interface consists of several key areas:

- **Canvas Area**: The central workspace where your image is displayed
- **Toolbar Tabs**: Located at the top, providing access to different editing tools
- **Tool Options Panel**: Context-sensitive options that appear based on the selected tool
- **Save Button**: Located in the upper-right corner to save your changes
- **Close Button**: Exit the editor (you'll be prompted if there are unsaved changes)

## Editing Tools

### Adjust

The Adjust tab provides basic image transformation tools:

#### Crop
- **Purpose**: Remove unwanted portions of your image or change its aspect ratio
- **How to use**:
  1. Click the **Crop** tool
  2. Drag the corner handles to adjust the crop area
  3. Move the entire crop box by clicking and dragging from the center
  4. Choose from preset aspect ratios (Classic TV 4:3, Cinemascope 21:9, etc.)
  5. Use social media presets for Facebook profile (180x180px) or cover photos (820x312px)
  6. Click **Apply** when satisfied with your crop

#### Rotate
- **Purpose**: Rotate your image in 90-degree increments or by custom angles
- **How to use**:
  1. Click the **Rotate** tool
  2. Use the slider to rotate the image incrementally
  3. The default increment is 90 degrees for quick orientation changes

#### Flip
- **Flip X**: Mirror the image horizontally (left-to-right)
- **Flip Y**: Flip the image vertically (top-to-bottom)
- **How to use**: Simply click the Flip X or Flip Y button to instantly flip your image

### Annotate

The Annotate tab allows you to add visual elements to your images:

#### Text
- **Purpose**: Add text labels, captions, or annotations to your image
- **How to use**:
  1. Click the **Text** tool
  2. Click on the image where you want to place text
  3. Type your text in the text box
  4. Customize font, size, color, and style in the options panel
  5. Drag to reposition or resize the text box

#### Drawing Tools
- **Pen**: Draw freeform lines and shapes
- **Arrow**: Add directional arrows to highlight specific areas
- **Line**: Draw straight lines
- **Rectangle**: Add rectangular shapes
- **Ellipse**: Add circular or oval shapes
- **Polygon**: Draw custom multi-sided shapes

**Common options for all drawing tools**:
- **Color**: Default fill color is red (#ff0000), but can be customized
- **Stroke Width**: Adjust the thickness of lines and borders
- **Opacity**: Control transparency of shapes and annotations

### Filters

Apply artistic and stylistic filters to your images:

- **Black & White**: Convert to grayscale
- **Sepia**: Apply a vintage, warm tone
- **Vintage**: Create an aged photograph look
- **Technicolor**: Enhance colors with a vibrant effect
- **Polaroid**: Emulate classic instant camera appearance
- **Kodachrome**: Simulate the famous film stock look

**How to use**:
1. Click on the **Filters** tab
2. Browse through available filter presets
3. Click any filter to apply it instantly
4. Compare with the original by toggling the preview
5. Adjust intensity if the option is available

### Finetune

Make precise adjustments to image properties:

- **Brightness**: Lighten or darken the image
- **Contrast**: Increase or decrease the difference between light and dark areas
- **Saturation**: Control color intensity
- **Hue**: Shift the overall color tone
- **Blur**: Apply a blur effect for softening or creating depth
- **Warmth**: Adjust the temperature (cooler blues or warmer oranges)
- **Gamma**: Control midtone brightness

**How to use**:
1. Select the **Finetune** tab
2. Use sliders to adjust individual properties
3. Changes apply in real-time as you move the sliders
4. Reset individual values or all changes using reset buttons

### Resize

Change the dimensions of your image:

**Resize Options**:
- **Width and Height**: Enter specific pixel dimensions
- **Maintain Aspect Ratio**: Lock the proportions to prevent distortion
- **Percentage**: Scale by a percentage of the original size
- **Preset Sizes**: Choose from common dimension presets

**How to use**:
1. Click the **Resize** tab
2. Enter your desired width and height (in pixels)
3. Ensure "Maintain Aspect Ratio" is checked to prevent distortion
4. Preview the changes before applying
5. Click **Apply** to resize

**Note**: Resizing down (making images smaller) preserves quality better than resizing up (enlarging).

### Watermark

Protect your images by adding watermarks:

#### Text Watermark
- Add text-based watermarks with customizable:
  - Font family and size
  - Color and opacity
  - Position (corners, center, or custom placement)
  - Rotation angle

#### Image Watermark
- Upload and place a logo or image as a watermark
- Control size, position, and opacity
- Useful for branding and copyright protection

**How to use**:
1. Select the **Watermark** tab
2. Choose between Text or Image watermark
3. For text: Enter your watermark text and customize appearance
4. For image: Upload your logo/watermark image
5. Position the watermark by dragging or using position presets
6. Adjust opacity to make it subtle or prominent

## Saving Your Work

When you're satisfied with your edits:

1. Click the **Save** button in the upper-right corner
2. The editor will automatically:
   - Process your changes
   - Save the modified image back to the File Manager
   - Preserve the original filename and format
   - Update the file in the same location

**Important Notes**:
- Saving overwrites the original image file
- If you want to keep the original, create a copy before editing
- The save process may take a few seconds for large images or complex edits
- You'll see a "Saving..." indicator while the file is being processed

## Supported Image Formats

The Image Editor supports the following formats:

- **PNG** (.png) - Best for graphics with transparency
- **JPEG/JPG** (.jpg, .jpeg) - Best for photographs
- **GIF** (.gif) - Supports animation (edits will affect first frame only)
- **WebP** (.webp) - Modern format with excellent compression

**Maximum File Size**: 25MB

## Best Practices

### Image Quality
- Work with the highest quality source images possible
- Avoid repeatedly editing and saving the same JPEG, as quality degrades with each save
- Use PNG for images requiring transparency
- Consider using WebP for optimal file size and quality balance

### Performance Tips
- Large images (over 5MB) may take longer to load and process
- Complex filters and effects require more processing time
- Close the editor when finished to free up browser resources

### Workflow Recommendations
1. **Backup originals**: Keep a copy of original images before editing
2. **Edit in stages**: For complex edits, save intermediate versions
3. **Test on different devices**: Preview how edited images look on various screen sizes
4. **Optimize file size**: Use the Resize tool to reduce dimensions for web use
5. **Use meaningful filenames**: Rename files to reflect their content and edits

### Accessibility Considerations
- When adding text annotations, ensure sufficient color contrast
- Use watermarks that don't obscure important image content
- Consider how filters affect readability and visibility
- Test edited images for clarity at different sizes

## Troubleshooting

### Common Issues

**Image Won't Load**
- Verify the file format is supported
- Check that the file size is under 25MB
- Ensure you have proper permissions to access the file

**Changes Not Saving**
- Make sure you clicked the Save button
- Check your internet connection
- Verify you have write permissions for the folder
- Try refreshing the page and editing again

**Editor Runs Slowly**
- Close other browser tabs to free up memory
- Work with smaller image dimensions when possible
- Disable browser extensions that might interfere
- Try using a different browser (Chrome or Edge recommended)

## Additional Resources

- **Filerobot Documentation**: https://scaleflex.github.io/filerobot-image-editor/
- **SkyCMS File Manager**: See File Manager documentation for file organization tips
- **Image Optimization**: Refer to the SkyCMS Storage Configuration guide

---

*For technical support or questions about the Image Editor, contact your SkyCMS administrator or refer to the main SkyCMS documentation.*
