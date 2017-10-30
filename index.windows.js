import {
  NativeModules,
} from 'react-native';

export default {
  createResizedImage: (path, width, height, format, quality, rotation = 0, outputPath) => {
    if (format !== 'JPEG' && format !== 'PNG') {
      throw new Error('Only JPEG and PNG format are supported by createResizedImage');
    }

    return NativeModules.ImageResizer.createResizedImage(path, width, height, format, quality, rotation, outputPath);
  },
};
