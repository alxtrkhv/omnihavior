set unstable
set script-interpreter := ['uv', 'run', '--script']

test:
  dotnet test Omnihavior.Tests/Omnihavior.Tests.csproj

build: test
  dotnet build --configuration release Omnihavior/Omnihavior.csproj

[script]
set-version new_version:
    import re
    import subprocess
    import sys
    from pathlib import Path

    new_version = "{{ new_version }}"
    csproj_path = Path("Omnihavior/Omnihavior.csproj")
    tag_name = f"release/v{new_version}"

    print(f"Setting version to {new_version} in {csproj_path}...")

    try:
        content = csproj_path.read_text()
        # Use regex to replace the version within the <Version> tag
        updated_content, count = re.subn(
            r"(<Version>)(.*?)(</Version>)",
            rf"\g<1>{new_version}\g<3>",
            content,
            count=1 # Ensure only the first match is replaced
        )

        if count == 0:
            print(f"Error: Could not find <Version> tag in {csproj_path}")
            sys.exit(1)

        csproj_path.write_text(updated_content)
        print(f"Successfully updated {csproj_path}")

        print(f"Creating git tag {tag_name}...")
        # Use check=True to raise an exception if the command fails
        subprocess.run(["git", "tag", tag_name], check=True, capture_output=True, text=True)
        print(f"Successfully created git tag {tag_name}")

    except FileNotFoundError:
        print(f"Error: {csproj_path} not found.")
        sys.exit(1)
    except subprocess.CalledProcessError as e:
        print(f"Error creating git tag {tag_name}:")
        print(e.stderr)
        # Optional: revert csproj change if tagging fails?
        # csproj_path.write_text(content)
        # print(f"Reverted changes in {csproj_path}")
        sys.exit(1)
    except Exception as e:
        print(f"An unexpected error occurred: {e}")
        sys.exit(1)

