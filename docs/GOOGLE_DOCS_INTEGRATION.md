# Google Docs Integration Guide

This guide explains how to set up and use the Google Docs integration in AuthManSys Console application.

## Table of Contents
- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Setup Instructions](#setup-instructions)
- [Configuration](#configuration)
- [Available Commands](#available-commands)
- [Command Examples](#command-examples)
- [Troubleshooting](#troubleshooting)

## Overview

The AuthManSys Console application includes comprehensive Google Docs integration that allows you to:
- Create new Google Documents
- Write content to existing documents
- List your Google Documents
- Get document information
- Share documents
- Export documents

## Prerequisites

- Google Cloud Console access
- Google Drive API and Google Docs API enabled
- Service Account with appropriate permissions

## Setup Instructions

### 1. Create Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Note your project ID for later use

### 2. Enable Required APIs

Enable the following APIs in your Google Cloud project:
- **Google Drive API**
- **Google Docs API**

To enable these APIs:
1. In Google Cloud Console, go to **APIs & Services** → **Library**
2. Search for "Google Drive API" and click **Enable**
3. Search for "Google Docs API" and click **Enable**

### 3. Create Service Account

1. Go to **IAM & Admin** → **Service Accounts**
2. Click **Create Service Account**
3. Enter details:
   - **Name**: `google-docs-manager` (or your preferred name)
   - **Description**: `Service account for AuthManSys Google Docs integration`
4. Click **Create and Continue**
5. Skip role assignment (we'll handle permissions via document sharing)
6. Click **Done**

### 4. Generate Service Account Key

1. Click on your newly created service account
2. Go to the **Keys** tab
3. Click **Add Key** → **Create New Key**
4. Select **JSON** format
5. Click **Create**
6. The JSON key file will be downloaded automatically

### 5. Install Service Account Key

1. Copy the downloaded JSON key file to your Console project's Storage directory:
   ```bash
   cp ~/Downloads/your-service-account-key.json /path/to/AuthManSys/src/AuthManSys.Console/Storage/laravel-proj-456719-e89abedab1b3.json
   ```

2. The `Storage/` folders are automatically excluded from git commits via `.gitignore`

## Configuration

The Google API configuration is located in `src/AuthManSys.Console/appsettings.json`:

```json
{
  "GoogleApi": {
    "ServiceAccountKeyPath": "src/AuthManSys.Console/Storage/laravel-proj-456719-e89abedab1b3.json",
    "ApplicationName": "AuthManSys",
    "Scopes": [
      "https://www.googleapis.com/auth/drive",
      "https://www.googleapis.com/auth/documents"
    ],
    "DefaultFolderId": "",
    "EnableLogging": true,
    "TimeoutSeconds": 30
  }
}
```

### Configuration Properties

- **ServiceAccountKeyPath**: Path to your service account JSON key file
- **ApplicationName**: Name of your application (used in Google API requests)
- **Scopes**: Required Google API scopes for Drive and Docs access
- **DefaultFolderId**: Optional - default Google Drive folder for new documents
- **EnableLogging**: Whether to enable detailed API logging
- **TimeoutSeconds**: Request timeout in seconds

## Available Commands

Run commands from the solution root directory using:
```bash
dotnet run --project src/AuthManSys.Console -- google [command] [options]
```

### Command List

| Command | Description | Syntax |
|---------|-------------|--------|
| `create` | Create a new Google Document | `google create <title>` |
| `write` | Write content to an existing document | `google write <document-id> <content>` |
| `create-with-content` | Create document with initial content | `google create-with-content <title> <content>` |
| `list` | List your Google Documents | `google list` |
| `info` | Get information about a document | `google info <document-id>` |
| `share` | Share a document with someone | `google share <document-id> <email> [--role <role>]` |
| `export` | Export a document | `google export <document-id> [--format <format>]` |

## Command Examples

### Create a New Document
```bash
dotnet run --project src/AuthManSys.Console -- google create "My New Document"
```

### Write Content to Existing Document
```bash
dotnet run --project src/AuthManSys.Console -- google write "1jOtVFDzHFnWlklf3WdF2Qyb4ryUhWqT3cUhS-1TnyEg" "Hello from AuthManSys!"
```

### Create Document with Initial Content
```bash
dotnet run --project src/AuthManSys.Console -- google create-with-content "Meeting Notes" "Meeting started at 9:00 AM"
```

### List All Documents
```bash
dotnet run --project src/AuthManSys.Console -- google list
```

### Get Document Information
```bash
dotnet run --project src/AuthManSys.Console -- google info "1jOtVFDzHFnWlklf3WdF2Qyb4ryUhWqT3cUhS-1TnyEg"
```

### Share Document
```bash
# Share as reader (default)
dotnet run --project src/AuthManSys.Console -- google share "1jOtVFDzHFnWlklf3WdF2Qyb4ryUhWqT3cUhS-1TnyEg" "user@example.com"

# Share as writer
dotnet run --project src/AuthManSys.Console -- google share "1jOtVFDzHFnWlklf3WdF2Qyb4ryUhWqT3cUhS-1TnyEg" "user@example.com" --role writer

# Share as editor
dotnet run --project src/AuthManSys.Console -- google share "1jOtVFDzHFnWlklf3WdF2Qyb4ryUhWqT3cUhS-1TnyEg" "user@example.com" --role editor
```

### Export Document
```bash
# Export as plain text (default)
dotnet run --project src/AuthManSys.Console -- google export "1jOtVFDzHFnWlklf3WdF2Qyb4ryUhWqT3cUhS-1TnyEg"

# Export as PDF
dotnet run --project src/AuthManSys.Console -- google export "1jOtVFDzHFnWlklf3WdF2Qyb4ryUhWqT3cUhS-1TnyEg" --format pdf
```

## Granting Document Access

To write to an existing Google Document, you must share it with your service account:

### Service Account Email
Your service account email from the JSON key file:
```
google-docs-manager@laravel-proj-456719.iam.gserviceaccount.com
```

### Steps to Share Document

1. Open the Google Document you want to write to
2. Click the **Share** button (top right corner)
3. Add the service account email: `google-docs-manager@laravel-proj-456719.iam.gserviceaccount.com`
4. Set permission level to **Editor** or **Writer**
5. Click **Send**

### Permission Levels
- **Viewer**: Can only read the document
- **Commenter**: Can read and comment
- **Editor/Writer**: Can read, write, and edit the document ✅ (Required for our commands)

## Troubleshooting

### Common Issues and Solutions

#### 1. "ServiceAccountKeyPath is not configured"
```
❌ Error: ServiceAccountKeyPath is not configured in GoogleApi settings
```
**Solution**:
- Ensure the JSON key file is in the Console project directory
- Verify the file name matches the `ServiceAccountKeyPath` in `appsettings.json`
- Check that the file is not corrupted

#### 2. "The caller does not have permission [403]"
```
❌ Error: The caller does not have permission [403]
```
**Solution**:
- Share the Google Document with your service account email
- Grant **Editor** or **Writer** permissions (not just Viewer)
- Wait a few minutes for permissions to propagate

#### 3. "Drive storage quota has been exceeded [403]"
```
❌ Error: The user's Drive storage quota has been exceeded
```
**Solution**:
- Free up space in your Google Drive
- Delete unnecessary files
- Consider upgrading your Google Drive storage plan

#### 4. "Service account key file not found"
```
❌ Error: Service account key file not found at: src/AuthManSys.Console/Storage/laravel-proj-456719-e89abedab1b3.json
```
**Solution**:
- Verify the JSON key file is in the correct directory: `src/AuthManSys.Console/Storage/`
- Check the file name matches exactly what's in `appsettings.json`
- Ensure proper file permissions

#### 5. API Rate Limiting
```
❌ Error: Quota exceeded for quota metric
```
**Solution**:
- Wait before retrying
- Consider implementing retry logic in your application
- Check your Google Cloud Console quotas and limits

### Getting Help

For additional support:

1. **Check Logs**: Enable detailed logging by setting `EnableLogging: true` in configuration
2. **Google Cloud Console**: Check your project's API usage and quotas
3. **Service Account**: Verify your service account has the necessary permissions
4. **Document Permissions**: Ensure documents are properly shared with the service account

### Security Best Practices

1. **Never commit service account keys to version control**
2. **Use environment-specific configuration files**
3. **Regularly rotate service account keys**
4. **Limit service account permissions to minimum required**
5. **Monitor API usage in Google Cloud Console**

## Example Output

### Successful Write Operation
```
✏️  Writing content to document 1jOtVFDzHFnWlklf3WdF2Qyb4ryUhWqT3cUhS-1TnyEg...
✅ Content written successfully!
   📝 Added 25 characters
```

### Document List
```
📋 Listing Google Documents...
   📄 Found 1 document(s):

   📌 google connect test doc
      ID: 1jOtVFDzHFnWlklf3WdF2Qyb4ryUhWqT3cUhS-1TnyEg
      Modified: 2025-12-25 08:29:08
      Link: https://docs.google.com/document/d/1jOtVFDzHFnWlklf3WdF2Qyb4ryUhWqT3cUhS-1TnyEg/edit?usp=drivesdk
```

---

*This integration was implemented as part of the AuthManSys Console application. For technical support or questions about the implementation, refer to the project documentation or contact the development team.*