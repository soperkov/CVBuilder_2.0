export interface Template {
  id: number;

  // Display & Identity
  name: string;
  description?: string | null;
  previewImageUrl: string;
  templateKey: string;

  // Rendering Configuration
  htmlTemplatePath: string;
  cssClass: string;
  fontFamily: string;
  primaryColor: string;
  accentColor?: string | null;

  // Layout Options
  isTwoColumn: boolean;
  paperSize: string;

  // Usage Flags
  isDefault: boolean;
  isActive: boolean;

  // Optional metadata
  category: string;
  notes?: string | null;
}
